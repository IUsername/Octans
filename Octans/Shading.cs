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
            return surface + reflected;
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
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (reflective == 0f)
            {
                return Colors.Black;
            }

            var reflectedRay = new Ray(info.OverPoint, info.Reflect);
            var color = ColorAt(world, reflectedRay, --remaining);
            return color * reflective;
        }
    }
}