using Octans.Reflection.Microfacet;
using static Octans.MathF;

namespace Octans.Reflection
{
    public sealed class MicrofacetReflection : IBxDF
    {
        public Spectrum R { get; private set; }
        public IMicrofacetDistribution Distribution { get; private set; }
        public IFresnel Fresnel { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Glossy;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var cosThetaO = AbsCosTheta(in wo);
            var cosThetaI = AbsCosTheta(in wi);
            var wh = wi + wo;

            // Handle degenerate cases.
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (cosThetaI == 0f || cosThetaO == 0f)
            {
                return Spectrum.Zero;
            }

            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            wh = wh.Normalize();
            var F = Fresnel.Evaluate(wi % wh);
            var D = Distribution.D(in wh);
            var G = Distribution.G(in wo, in wi);
            return Spectrum.FusedMultiply(F, R, (D * G / (4f * cosThetaI * cosThetaO)));
           // return  (D * G / (4f * cosThetaI * cosThetaO)) * (R * F);
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

            var wh = Distribution.SampleWh(wo, u);
            wi = Reflect(wo, wh);
            if (!IsInSameHemisphere(wo, wi))
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            pdf = Distribution.Pdf(wo, wh) / (4f * wo % wh);
            return F(wo, wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi)
        {
            if (!IsInSameHemisphere(wo, wi))
            {
                return 0f;
            }

            var wh = (wo + wi).Normalize();
            return Distribution.Pdf(wo, wi) / (4f * wo % wh);
        }

        public MicrofacetReflection Initialize(in Spectrum r, IMicrofacetDistribution distribution, IFresnel fresnel)
        {
            R = r;
            Distribution = distribution;
            Fresnel = fresnel;
            return this;
        }
    }
}