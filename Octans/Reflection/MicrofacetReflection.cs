using System;
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
            if (cosThetaI == 0 || cosThetaO == 0)
            {
                return Spectrum.Zero;
            }

            if (wh.X == 0 && wh.Y == 0 && wh.Z == 0)
            {
                return Spectrum.Zero;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            wh = wh.Normalize();
            var F = Fresnel.Evaluate(wi % wh);
            return R * Distribution.D(in wh) * Distribution.G(in wo, in wi) * F / (4f * cosThetaI * cosThetaO);
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) =>
            Utilities.Rho(this, nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi)
        {
            if (!Utilities.IsInSameHemisphere(wo, wi))
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