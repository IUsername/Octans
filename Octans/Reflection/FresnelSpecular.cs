using System;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class FresnelSpecular : IBxDF
    {
        private readonly FresnelDielectric _fresnel = new FresnelDielectric();

        public FresnelSpecular(in Spectrum r, in Spectrum t, float etaA, float etaB, TransportMode mode)
        {
            R = r;
            T = t;
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
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) =>
            IsInSameHemisphere(wo, wi) ? AbsCosTheta(wi) * InvPi : 0f;
    }
}