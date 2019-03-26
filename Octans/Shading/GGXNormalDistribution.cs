using System;

namespace Octans.Shading
{
    public class GGXNormalDistribution : INormalDistribution
    {
        public static INormalDistribution Instance = new GGXNormalDistribution();

        private GGXNormalDistribution()
        {
        }

        public float Factor(in ShadingInfo info) => GGXNormalDist(info.Alpha, info.Roughness, info.NdotH);

        private static float GGXNormalDist(float alpha, float roughness, float NdotH)
        {
            const float oneOverPi = 1f / MathF.PI;
            var NdotHSqr = NdotH * NdotH;
            var tanNdotHSqr = (1 - NdotHSqr) / NdotHSqr;
            var s = roughness / (NdotHSqr * (alpha + tanNdotHSqr));
            if (s > 0)
            {
                return oneOverPi * MathF.Sqrt(s);
            }

            return 0f;
        }
    }
}