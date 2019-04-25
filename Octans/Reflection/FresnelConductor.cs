using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class FresnelConductor : IFresnel
    {
        public Spectrum EtaI { get; private set; }
        public Spectrum EtaT { get; private set; }
        public Spectrum K { get; private set; }

        public Spectrum Evaluate(float cosI) => FrConductor(Abs(cosI), EtaI, EtaT, K);

        public IFresnel Initialize(in Spectrum etaI, in Spectrum etaT, in Spectrum k)
        {
            EtaI = etaI;
            EtaT = etaT;
            K = k;
            return this;
        }

        public static Spectrum FrConductor(float cosThetaI, in Spectrum etaI, in Spectrum etaT, in Spectrum k)
        {
            cosThetaI = Clamp(-1, 1, cosThetaI);
            var eta = etaT / etaI;
            var etaK = k / etaI;

            var cosThetaI2 = cosThetaI * cosThetaI;
            var sinThetaI2 = 1f - cosThetaI2;
            var eta2 = eta * eta;
            var etaK2 = etaK * etaK;

            var t0 = eta2 - etaK2 - sinThetaI2;
            var a2Pb2 = (t0 * t0 + 4f * eta2 * etaK2).Sqrt();
            var t1 = a2Pb2 + cosThetaI2;
            var a = (0.5f * (a2Pb2 + t0)).Sqrt();
            var t2 = 2f * cosThetaI * a;
            var rPerpendicular = (t1 - t2) / (t1 + t2);
            var t3 = cosThetaI2 * a2Pb2 + sinThetaI2 * sinThetaI2;
            var t4 = t2 * sinThetaI2;
            var rParallel = rPerpendicular * ((t3 - t4) / (t3 + t4));

            return 0.5f * (rParallel + rPerpendicular);
        }
    }
}