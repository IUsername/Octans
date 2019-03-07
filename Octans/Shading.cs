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

        public static Color Lighting(Material m,
                                     IShape shape,
                                     PointLight light,
                                     Point position,
                                     Vector eyeVector,
                                     Vector normalVector,
                                     bool inShadow)
        {
            var effectiveColor = m.Pattern?.ColorAt(position, shape) ?? m.Color * light.Intensity;
            var ambient = effectiveColor * m.Ambient;

            if (inShadow)
            {
                return ambient;
            }

            var lightV = (light.Position - position).Normalize();
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

        public static Color HitColor(World world, in IntersectionInfo info)
        {
            var color = Colors.Black;
            foreach (var light in world.Lights)
            {
                var isShadowed = IsShadowed(world, info.OverPoint);
                // TODO: Use OverPoint here?
                color += Lighting(info.Shape.Material, info.Shape, light, info.OverPoint, info.Eye, info.Normal, isShadowed);
            }
            return color;
        }

        public static Color ColorAt(World world, in Ray ray)
        {
            var xs = world.Intersect(ray);
            var hit = xs.Hit();
            if (!hit.HasValue)
            {
                return Colors.Black;
            }

            var info = new IntersectionInfo(hit.Value, ray);
            return HitColor(world, info);
        }
    }
}