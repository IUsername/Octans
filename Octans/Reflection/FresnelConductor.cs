using System;
using static Octans.Reflection.Utilities;

namespace Octans.Reflection
{
    public class FresnelConductor : IFresnel
    {
        private readonly Spectrum _etaI;
        private readonly Spectrum _etaT;
        private readonly Spectrum _k;

        public FresnelConductor(in Spectrum etaI, in Spectrum etaT, in Spectrum k)
        {
            _etaI = etaI;
            _etaT = etaT;
            _k = k;
        }

        public Spectrum Evaluate(float cosI)
        {
            return FrConductor(System.MathF.Abs(cosI), _etaI, _etaT, _k);
        }
    }
}