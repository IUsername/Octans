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
            var diffuse = 28f / (23f * PI) * Rd * (Spectrum.One - Rs) *
                          (1f - Pow5(1f - 0.5f * AbsCosTheta(in wi))) *
                          (1f - Pow5(1f - 0.5f * AbsCosTheta(in wo)));
            var specular = Distribution.D(in wh) /
                           (4f * AbsDot(in wi, in wh) * Max(AbsCosTheta(in wi), AbsCosTheta(in wo))) *
                           SchlickFresnelFunction(wi % wh);
            return diffuse + specular;
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);

        public Spectrum SchlickFresnelFunction(float cosTheta)
        {
            static float Pow5(float v) => v * v * (v * v) * v;
            return Rs + Pow5(1f - cosTheta) * (Spectrum.One - Rs);
        }
    }
}