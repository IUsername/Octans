using static Octans.Reflection.Utilities;

namespace Octans.Reflection
{
    public class FresnelDielectric : IFresnel
    {
        private readonly float _etaI;
        private readonly float _etaT;

        public FresnelDielectric(float etaI, float etaT)
        {
            _etaI = etaI;
            _etaT = etaT;
        }

        public Spectrum Evaluate(float cosI)
        {
            return new Spectrum(FrDielectric(cosI, _etaI, -_etaT));
        }
    }
}