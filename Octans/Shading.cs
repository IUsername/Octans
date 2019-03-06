using System;

namespace Octans
{
    public static class Shading
    {
        public static Color Lighting(Material m, PointLight light, Point position, Vector eyeVector, Vector normalVector)
        {
            var effectiveColor = m.Color * light.Intensity;
            var lightV = (light.Position - position).Normalize();
            var ambient = effectiveColor * m.Ambient;
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
    }
}