using System;

namespace Octans
{
    public static class Shading
    {
        public static bool IsShadowed(World w, Point p)
        {
            // TODO: Only supports one light.
            var light = w.Lights[0];
            var v = light.Position - p;
            var distance = v.Magnitude();
            var direction = v.Normalize();
            var r = new Ray(p, direction);
            var xs = w.Intersect(r);
            var h = xs.Hit();
            return h.HasValue && h.Value.T < distance;
        }

        public static Color ShapeColor(this IPattern pattern, IShape shape, Point worldPoint)
        {
            var local = worldPoint.ToLocal(shape, pattern);
            return pattern.LocalColorAt(local);
        }

        public static Color Lighting(Material m,
                                     IShape shape,
                                     PointLight light,
                                     Point worldPoint,
                                     Vector eyeVector,
                                     Vector normalVector,
                                     bool inShadow)
        {
            var effectiveColor = m.Pattern.ShapeColor(shape, worldPoint) * light.Intensity;
            var ambient = effectiveColor * m.Ambient;

            if (inShadow)
            {
                return ambient;
            }

            var lightV = (light.Position - worldPoint).Normalize();
            var lightDotNormal = lightV % normalVector;

            if (!(lightDotNormal >= 0f))
            {
                return ambient;
            }

            var diffuse = effectiveColor * m.Diffuse * lightDotNormal;

            var reflectV = -lightV.Reflect(normalVector);
            var reflectDotEye = reflectV % eyeVector;
            if (!(reflectDotEye > 0f))
            {
                return ambient + diffuse;
            }

            var factor = MathF.Pow(reflectDotEye, m.Shininess);
            var specular = light.Intensity * m.Specular * factor;
            return ambient + diffuse + specular;
        }

        public static Color HitColor(World world, in IntersectionInfo info, int remaining = 5)
        {
            var surface = Colors.Black;
            foreach (var light in world.Lights)
            {
                var isShadowed = IsShadowed(world, info.OverPoint);
                // TODO: Use OverPoint here?
                surface += Lighting(info.Shape.Material,
                                    info.Shape,
                                    light,
                                    info.OverPoint,
                                    info.Eye,
                                    info.Normal,
                                    isShadowed);
            }

            var reflected = ReflectedColor(world, info, remaining);
            var refracted = RefractedColor(world, info, remaining);

            var material = info.Shape.Material;
            if (material.Reflective > 0f && material.Transparency > 0f)
            {
                var reflectance = Schlick(info);
                return surface + reflected * reflectance + refracted * (1f-reflectance);
            }
            return surface + reflected + refracted;
        }

        public static Color ColorAt(World world, in Ray ray, int remaining = 5)
        {
            var xs = world.Intersect(ray);
            var hit = xs.Hit();
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

            var reflective = info.Shape.Material.Reflective;
            if (reflective <= 0f)
            {
                return Colors.Black;
            }

            var reflectedRay = new Ray(info.OverPoint, info.Reflect);
            var color = ColorAt(world, reflectedRay, --remaining);
            return color * reflective;
        }

        public static Color RefractedColor(World world, in IntersectionInfo info, int remaining)
        {
            if (remaining < 1)
            {
                return Colors.Black;
            }

            if (info.Shape.Material.Transparency <= 0f)
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
            var direction = info.Normal * (nRatio * cosI - cosT) - info.Eye * nRatio;
            var refractedRay = new Ray(info.UnderPoint, direction);
            return ColorAt(world, refractedRay, --remaining) * info.Shape.Material.Transparency;
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
                if( sin2T > 1f)
                {
                    return 1.0f;
                }

                // Cosine of theta_t
                var cosT = MathF.Sqrt(1f - sin2T);
                cos = cosT;
            }

            var r0 = (info.N1 - info.N2) / (info.N1 + info.N2);
            r0 *= r0;
            return r0 + (1f - r0) * MathF.Pow(1 - cos, 5);
        }
    }
}