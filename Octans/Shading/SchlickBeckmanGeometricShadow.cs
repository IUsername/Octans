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
}