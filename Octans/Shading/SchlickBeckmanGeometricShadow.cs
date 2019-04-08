using System;

namespace Octans.Shading
{
    public class SchlickBeckmanGeometricShadow : IGeometricShadow
    {
        public static IGeometricShadow Instance = new SchlickBeckmanGeometricShadow();

        private SchlickBeckmanGeometricShadow()
        {
        }

        public float Factor(in ShadingInfo info) => SchlickBeckmanGSF(info.Alpha, info.NdotL, info.NdotV);

        private static float SchlickBeckmanGSF(float alpha, float NdotL, float NdotV)
        {
            const float sqrtTwoOverPi = 0.797884560802865f;
            var k = alpha * sqrtTwoOverPi;

            var smithL = NdotL / (NdotL * (1f - k) + k);
            var smithV = NdotV / (NdotV * (1f - k) + k);
            var gsf = smithL + smithV;
            return gsf;
        }
    }

    public class GGXSmithGeometricShadow : IGeometricShadow
    {
        public static IGeometricShadow Instance = new GGXSmithGeometricShadow();

        private GGXSmithGeometricShadow()
        {
        }

        public float Factor(in ShadingInfo info)
        {
            var Gl = GGX(info.Alpha, info.NdotL);
            var Gv = GGX(info.Alpha, info.NdotV);
            return Gl * Gv;
        }

        private static float GGX(float alpha, float NdotX)
        {
            var t = MathF.Sqrt(alpha + (1f - alpha) * (NdotX * NdotX));
            return 2 * NdotX / (NdotX + t);
        }
    }
}