using System;
using Octans.Light;

namespace Octans.Shading
{
    public static class PhongShading
    {
        public static bool IsShadowed(World w, in Point p, in Point lightPoint)
        {
            var v = lightPoint - p;
            var distance = v.Magnitude();
            var direction = v.Normalize();
            var r = new Ray(p, direction);
            var xs = w.Intersect(in r);
            var h = xs.Hit(true);
            xs.Return();
            return h.HasValue && h.Value.T < distance;
        }

        public static Color ShapeColor(this ITexture texture, IGeometry geometry, Point worldPoint)
        {
            var local = worldPoint.ToLocal(geometry, texture);
            return texture.LocalColorAt(in local);
        }

        public static Color Lighting(Material m,
                                     IGeometry geometry,
                                     ILight light,
                                     Point worldPoint,
                                     Vector eyeVector,
                                     Normal normalVector,
                                     float intensity)
        {
            var effectiveColor = m.Texture.ShapeColor(geometry, worldPoint) * light.Intensity;
            var ambient = effectiveColor * m.Ambient;

            if (intensity == 0.0f)
            {
                return ambient;
            }

            var sum = Colors.Black;
            foreach (var lightPoint in light.SamplePoints)
            {
                var lightV = (lightPoint - worldPoint).Normalize();
                var lightDotNormal = lightV % normalVector;

                if (lightDotNormal >= 0f)
                {
                    var diffuse = effectiveColor * m.Diffuse * lightDotNormal;
                    sum += diffuse;

                    var reflectV = -lightV.Reflect(normalVector);
                    var reflectDotEye = reflectV % eyeVector;
                    if (reflectDotEye > 0f)
                    {
                        var factor = MathF.Pow(reflectDotEye, m.Shininess);
                        var specular = light.Intensity * m.Specular * factor;
                        sum += specular;
                    }
                }
            }

            return ambient + sum / light.Samples * intensity;
        }

        public static Color HitColor(World world, in IntersectionInfo info, int remaining = 5)
        {
            var surface = Colors.Black;
            foreach (var light in world.Lights)
            {
                var intensity = IntensityAt(world, info.OverPoint, light);
                // TODO: Use OverPoint here?
                surface += Lighting(info.Geometry.Material,
                                    info.Geometry,
                                    light,
                                    info.OverPoint,
                                    info.Eye,
                                    info.Normal,
                                    intensity);
            }

            var reflected = ReflectedColor(world, info, remaining);
            var refracted = RefractedColor(world, info, remaining);

            var material = info.Geometry.Material;
            if (material.Reflective > 0f && material.Transparency > 0f)
            {
                var reflectance = Schlick(in info);
                return surface + reflected * reflectance + refracted * (1f - reflectance);
            }

            return surface + reflected + refracted;
        }

        public static Color ColorAt(World world, in Ray ray, int remaining = 5)
        {
            var xs = world.Intersect(in ray);
            var hit = xs.Hit();
            xs.Return();
            if (!hit.HasValue)
            {
                return Colors.Black;
            }

            var info = new IntersectionInfo(hit.Value, ray);
            return HitColor(world, info, remaining);
        }

        public static Color ReflectedColor(World world, in IntersectionInfo info, int remaining = 5)
        {
            if (remaining < 1)
            {
                return Colors.Black;
            }

            var reflective = info.Geometry.Material.Reflective;
            if (reflective <= 0f)
            {
                return Colors.Black;
            }

            var reflectedRay = new Ray(info.OverPoint, info.Reflect);
            var color = ColorAt(world, in reflectedRay, --remaining);
            return color * reflective;
        }

        public static Color RefractedColor(World world, in IntersectionInfo info, int remaining)
        {
            if (remaining < 1)
            {
                return Colors.Black;
            }

            if (info.Geometry.Material.Transparency <= 0f)
            {
                return Colors.Black;
            }

            var nRatio = info.N1 / info.N2;
            var cosI = info.Eye % info.Normal;
            var sin2T = nRatio * nRatio * (1f - cosI * cosI);
            if (sin2T > 1f)
            {
                // Total internal reflection.
                return Colors.Black;
            }

            var cosT = MathF.Sqrt(1f - sin2T);
            var direction = (Vector)info.Normal * (nRatio * cosI - cosT) - info.Eye * nRatio;
            var refractedRay = new Ray(info.UnderPoint, direction);
            return ColorAt(world, in refractedRay, --remaining) * info.Geometry.Material.Transparency;
        }

        public static float Schlick(in IntersectionInfo info)
        {
            // Cosine of angle between eye and normal vectors.
            var cos = info.Eye % info.Normal;

            // Total internal reflections can only occur when N1 > N2.
            if (info.N1 > info.N2)
            {
                var n = info.N1 / info.N2;
                var sin2T = n * n * (1f - cos * cos);
                if (sin2T > 1f)
                {
                    return 1.0f;
                }

                // Cosine of theta_t
                var cosT = MathF.Sqrt(1f - sin2T);
                cos = cosT;
            }

            var r0 = (info.N1 - info.N2) / (info.N1 + info.N2);
            r0 *= r0;
            // Probability of reflection
            var rProb = r0 + (1f - r0) * MathF.Pow(1 - cos, 5);
            return rProb;
        }

        public static float IntensityAt(World world, in Point point, ILight light)
        {
            switch (light)
            {
                case PointLight _:
                    return IsShadowed(world, in point, light.Position) ? 0.0f : 1.0f;
                case AreaLight area:
                {
                    var total = 0.0f;
                    for (var v = 0; v < area.VSteps; v++)
                    {
                        for (var u = 0; u < area.USteps; u++)
                        {
                            if (!IsShadowed(world, in point, area.UVPoint(u, v)))
                            {
                                total += 1.0f;
                            }
                        }
                    }

                    return total / area.Samples;
                }
                default:
                    return 0f;
            }
        }
    }
}