using static System.MathF;
using static Octans.Reflection.Utilities;

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
    }
}