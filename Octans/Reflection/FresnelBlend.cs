using System;
using Octans.Reflection.Microfacet;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class FresnelBlend : IBxDF
    {
        public FresnelBlend(in Spectrum rd, in Spectrum rs, IMicrofacetDistribution distribution)
        {
            Rd = rd;
            Rs = rs;
            Distribution = distribution;
        }

        public Spectrum Rd { get; }
        public Spectrum Rs { get; }
        public IMicrofacetDistribution Distribution { get; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Glossy;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            static float Pow5(float v) => v * v * (v * v) * v;

            var wh = wi + wo;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (wh.X == 0 && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            wh = wh.Normalize();

            var diffuse = 28f / (23f * PI) *
                          (1f - Pow5(1f - 0.5f * AbsCosTheta(in wi))) *
                          (1f - Pow5(1f - 0.5f * AbsCosTheta(in wo))) *
                          Rd * (Spectrum.One - Rs);

            var specular = Distribution.D(in wh) /
                           (4f * AbsDot(in wi, in wh) * Max(AbsCosTheta(in wi), AbsCosTheta(in wo))) *
                           SchlickFresnelFunction(wi % wh);

            return diffuse + specular;
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi)
        {
            if (!IsInSameHemisphere(wo, wi))
            {
                return 0f;
            }

            var wh = (wo + wi).Normalize();
            var pdf_wh = Distribution.Pdf(wo, wh);
            return 0.5f * (AbsCosTheta(wi) * InvPi + pdf_wh / (4f * wo % wh));
        }

        public Spectrum SchlickFresnelFunction(float cosTheta)
        {
            static float Pow5(float v) => v * v * (v * v) * v;
            return Rs + Pow5(1f - cosTheta) * (Spectrum.One - Rs);
        }
    }
}