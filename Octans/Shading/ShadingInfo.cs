namespace Octans.Shading
{
    public readonly struct ShadingInfo
    {
        public ShadingInfo(float lightIntensity,
                           in ILight light,
                           in IntersectionInfo intersection,
                           in Point sampled)
        {
            Eye = intersection.Eye;
            NormalDirection = (Vector) intersection.Normal;
            LightDirection = (sampled - intersection.OverPoint).Normalize();
            LightReflectDirection = -LightDirection.Reflect(NormalDirection);
            ViewDirection = intersection.Eye;
            ViewReflectDirection = -ViewDirection.Reflect(NormalDirection);
            HalfDirection = (ViewDirection + LightDirection).Normalize();
            NdotL = MathFunction.Saturate(NormalDirection % LightDirection);
            NdotH = MathFunction.Saturate(NormalDirection % HalfDirection);
            NdotV = MathFunction.Saturate(NormalDirection % ViewDirection);
            VdotH = MathFunction.Saturate(ViewDirection % HalfDirection);
            LdotH = MathFunction.Saturate(LightDirection % HalfDirection);
            RdotV = MathFunction.Saturate(LightReflectDirection % ViewDirection);

            // TODO: Attenuation properties
            LightIntensity = lightIntensity * light.Intensity;

            var material = intersection.Geometry.Material;

            Roughness = intersection.Roughness;

            Metallic = material.Metallic;

            // Disney approach to Lambertian diffuse
            F0 = FZero(NdotL, NdotV, LdotH, Roughness);

            // TODO: Albedo??
            var color =
                intersection.Geometry.Material.Texture.ShapeColor(intersection.Geometry, intersection.OverPoint);
            DiffuseColor = (1f - Metallic) * color * F0;

            //// TODO: Is this a good assumption?
            SpecularColor = Color.Lerp(material.SpecularColor, color, Metallic * 0.5f);
            //SpecularColor = material.SpecularColor;

            // TODO: How to determine this?
            IoR = 2f;

            Alpha = intersection.Alpha;
        }

        public float Alpha { get; }

        public float F0 { get; }

        private static float SchlickFresnel(float i)
        {
            var x = MathFunction.Saturate(1f - i);
            var x2 = x * x;
            return x2 * x2 * x;
        }

        private static float FZero(float NdotL, float NdotV, float LdotH, float roughness)
        {
            var fresnelLight = SchlickFresnel(NdotL);
            var fresnelView = SchlickFresnel(NdotV);
            var fresnelDiffuse = 0.5f + 2f * LdotH * LdotH * roughness;
            return MathFunction.Lerp(1f, fresnelDiffuse, fresnelLight) *
                   MathFunction.Lerp(1f, fresnelDiffuse, fresnelView);
        }

        public float IoR { get; }

        public Color SpecularColor { get; }

        public Color DiffuseColor { get; }

        public float Metallic { get; }

        public float Roughness { get; }

        public Color LightIntensity { get; }

        public float RdotV { get; }

        public float LdotH { get; }

        public float VdotH { get; }

        public float NdotV { get; }

        public float NdotH { get; }

        public float NdotL { get; }

        public Vector HalfDirection { get; }

        public Vector ViewReflectDirection { get; }

        public Vector ViewDirection { get; }

        public Vector LightReflectDirection { get; }

        public Vector LightDirection { get; }

        public Vector NormalDirection { get; }
        public Vector Eye { get; }
    }
}