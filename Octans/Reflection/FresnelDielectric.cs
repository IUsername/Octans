using static Octans.Reflection.Utilities;

namespace Octans.Reflection
{
    public class FresnelDielectric : IFresnel
    {
        private float _etaI;
        private float _etaT;

        public IFresnel Initialize(float etaI, float etaT)
        {
            _etaI = etaI;
            _etaT = etaT;
            return this;
        }

        public Spectrum Evaluate(float cosI) => new Spectrum(FrDielectric(cosI, _etaI, -_etaT));
    }
}