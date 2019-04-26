using Octans.Reflection.Microfacet;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class MicrofacetTransmission : IBxDF
    {
        private float EtaA { get; set; }
        private float EtaB { get; set; }

        public TransportMode Mode { get; private set; }

        public Spectrum T { get; private set; }
        public IMicrofacetDistribution Distribution { get; private set; }
        public FresnelDielectric Fresnel { get; } = new FresnelDielectric();

        public BxDFType Type => BxDFType.Transmission | BxDFType.Glossy;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            // Only handle transmission
            if (IsInSameHemisphere(in wo, in wi))
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

            var eta = CosTheta(in wo) > 0f ? EtaB / EtaA : EtaA / EtaB;
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
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            if (wo.Z == 0f)
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            var wh = Distribution.SampleWh(in wo, in u);
            var eta = CosTheta(in wo) > 0f ? EtaB / EtaA : EtaA / EtaB;
            if (!Refract(wo, (Normal) wh, eta, ref wi))
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            pdf = Pdf(wo, wi);
            return F(wo, wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi)
        {
            if (IsInSameHemisphere(wo, wi))
            {
                return 0f;
            }

            var eta = CosTheta(wo) > 0f ? EtaB / EtaA : EtaA / EtaB;
            var wh = (wo + wi * eta).Normalize();

            var sqrtDenom = wo % wh + eta * wi % wh;
            var dwh_dwi = Abs(eta * eta * wi % wh) / (sqrtDenom * sqrtDenom);
            return Distribution.Pdf(wo, wh) * dwh_dwi;
        }

        public MicrofacetTransmission Initialize(in Spectrum t,
                                                 IMicrofacetDistribution distribution,
                                                 float etaA,
                                                 float etaB,
                                                 TransportMode mode)
        {
            EtaA = etaA;
            EtaB = etaB;
            Mode = mode;
            T = t;
            Distribution = distribution;
            Fresnel.Initialize(etaA, etaB);
            return this;
        }
    }
}