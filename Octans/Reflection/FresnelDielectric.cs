using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class FresnelDielectric : IFresnel
    {
        public float EtaI { get; private set; }
        public float EtaT { get; private set; }

        public Spectrum Evaluate(float cosThetaI) => new Spectrum(FrDielectric(cosThetaI, EtaI, EtaT));

        public IFresnel Initialize(float etaI, float etaT)
        {
            EtaI = etaI;
            EtaT = etaT;
            return this;
        }

        public static float FrDielectric(float cosThetaI, float etaI, float etaT)
        {
            cosThetaI = Clamp(-1, 1, cosThetaI);
            var isEntering = cosThetaI > 0f;
            if (!isEntering)
            {
                (etaI, etaT) = (etaT, etaI);
                cosThetaI = Abs(cosThetaI);
            }

            var sinThetaI = Sqrt(Max(0f, 1 - cosThetaI * cosThetaI));
            var sinThetaT = etaI / etaT * sinThetaI;

            if (sinThetaT > 1f)
            {
                // Total internal reflection.
                return 1f;
            }

            var cosThetaT = Sqrt(Max(0f, 1 - sinThetaT * sinThetaT));

            var rParallel = (etaT * cosThetaI - etaI * cosThetaT) /
                            (etaT * cosThetaI + etaI * cosThetaT);

            var rPerpendicular = (etaI * cosThetaI - etaT * cosThetaT) /
                                 (etaI * cosThetaI + etaT * cosThetaT);
            return (rParallel * rParallel + rPerpendicular * rPerpendicular) / 2f;
        }
    }
}