﻿namespace Octans
{
    public readonly struct ShadingInfo
    {
        public ShadingInfo(float lightIntensity,
                           in ILight light,
                           in IntersectionInfo intersection)
        {
            NormalDirection = intersection.Normal;
            LightDirection = (light.Position - intersection.OverPoint).Normalize();
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
            AttenuationColor = lightIntensity * light.Intensity;

            var material = intersection.Shape.Material;

            Roughness = material.Roughness;

            Metallic = material.Metallic;

            F0 = FZero(NdotL, NdotV, LdotH, Roughness);

            // TODO: Albedo??
            var color = intersection.Shape.Material.Pattern.ShapeColor(intersection.Shape, intersection.OverPoint);
            DiffuseColor = (1f - Metallic) * color;// * F0; // F0? Should self-shadow already account for this?

            SpecularColor = Color.Lerp(color, material.SpecularColor, Metallic * 0.5f);

            // TODO: How to determine this?
            IoR = 2f;

            Alpha = Roughness * Roughness;
        }

        public float Alpha { get; }

        public float F0 { get; }

        private static float SchlickFresnel(float i)
        {
            var x = MathFunction.ClampF(0f, 1f, 1f - i);
            var x2 = x * x;
            return x2 * x2 * x;
        }

        private static float FZero(float NdotL, float NdotV, float LdotH, float roughness)
        {
            var fresnelLight = SchlickFresnel(NdotL);
            var fresnelView = SchlickFresnel(NdotV);
            var fresnelDiffuse = 0.5f + 2f * LdotH * LdotH * roughness;
            return MathFunction.MixF(1f, fresnelDiffuse, fresnelLight) *
                   MathFunction.MixF(1f, fresnelDiffuse, fresnelView);
        }

        public float IoR { get; }

        public Color SpecularColor { get; }

        public Color DiffuseColor { get; }

        public float Metallic { get; }

        public float Roughness { get; }

        public Color AttenuationColor { get; }

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
    }
}