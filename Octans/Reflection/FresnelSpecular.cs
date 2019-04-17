using System;

namespace Octans.Reflection
{
    public class FresnelSpecular : IBxDF
    {
        private readonly float _etaA;
        private readonly float _etaB;
        private readonly FresnelDielectric _fresnel = new FresnelDielectric();

        public FresnelSpecular(in Spectrum r, in Spectrum t, float etaA, float etaB, TransportMode mode)
        {
            R = r;
            T = t;
            _etaA = etaA;
            _etaB = etaB;
            Mode = mode;
            _fresnel.Initialize(etaA, etaB);
        }

        public Spectrum R { get; }
        public Spectrum T { get; }
        public TransportMode Mode { get; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Transmission | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);
    }
}