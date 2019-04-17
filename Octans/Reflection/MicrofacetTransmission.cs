using System;
using Octans.Reflection.Microfacet;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class MicrofacetTransmission : IBxDF
    {
        private readonly float _etaA;
        private readonly float _etaB;

        public MicrofacetTransmission(in Spectrum t,
                                      IMicrofacetDistribution distribution,
                                      float etaA,
                                      float etaB,
                                      TransportMode mode)
        {
            _etaA = etaA;
            _etaB = etaB;
            Mode = mode;
            T = t;
            Distribution = distribution;
            Fresnel.Initialize(etaA, etaB);
        }

        public TransportMode Mode { get; }

        public Spectrum T { get; }
        public IMicrofacetDistribution Distribution { get; }
        public FresnelDielectric Fresnel { get; } = new FresnelDielectric();

        public BxDFType Type => BxDFType.Transmission | BxDFType.Glossy;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            // Only handle transmission
            if (Utilities.IsInSameHemisphere(in wo, in wi))
            {
                return Spectrum.Zero;
            }

            var cosThetaO = CosTheta(in wo);
            var cosThetaI = CosTheta(in wi);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (cosThetaI == 0 || cosThetaO == 0)
            {
                return Spectrum.Zero;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var eta = CosTheta(in wo) > 0f ? _etaB / _etaA : _etaA / _etaB;
            var wh = (wo + wi * eta).Normalize();
            if (wh.Z < 0)
            {
                wh = -wh;
            }

            var F = Fresnel.Evaluate(wo % wh);
            var sqtDenom = wo % wh + eta * (wi % wh);
            var factor = Mode == TransportMode.Radiance ? 1f / eta : 1f;

            return (Spectrum.One - F) * T * Abs(Distribution.D(in wh) * Distribution.G(in wo, in wi) * eta * eta *
                                                AbsDot(in wo, in wh) * factor * factor /
                                                (cosThetaI * cosThetaO * sqtDenom * sqtDenom));
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);
    }
}