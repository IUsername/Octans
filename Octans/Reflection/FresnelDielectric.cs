﻿using static Octans.Reflection.Utilities;

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
    }
}