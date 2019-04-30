using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;
using static Octans.MathF;
using static Octans.Sampling.Utilities;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Octans.Reflection
{
    public static class BxDFExtensions
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoughnessToAlpha(this float roughness)
        {
            roughness = Max(roughness, 1e-3f);
            var x = Log(roughness);
            return 1.62142f + 0.819955f * x + 0.1734f * x * x + 0.0171201f * x * x * x + 0.000640711f * x * x * x * x;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Spectrum RhoValue(this IBxDF bxdf, in Vector wo, int nSamples, in Point2D[] u)
        {
            var r = Spectrum.Zero;
            for (var i = 0; i < nSamples; i++)
            {
                var wi = new Vector();
                var f = bxdf.SampleF(in wo, ref wi, u[i], out var pdf);
                if (pdf > 0f)
                {
                    r += f * AbsCosTheta(in wi) / pdf;
                }
            }

            return r / nSamples;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Spectrum RhoValue(this IBxDF bxdf, int nSamples, in Point2D[] u1, in Point2D[] u2)
        {
            var r = Spectrum.Zero;
            for (var i = 0; i < nSamples; i++)
            {
                var wi = new Vector();
                var wo = UniformSampleHemisphere(u1[i]);
                var pdfo = UniformHemispherePdf();
                var f = bxdf.SampleF(in wo, ref wi, u2[i], out var pdfi);
                if (pdfi > 0f)
                {
                    r += f * AbsCosTheta(in wi) * AbsCosTheta(in wo) / (pdfo * pdfi);
                }
            }

            return r / (PI * nSamples);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LambertianPdfValue(this IBxDF bxdf, in Vector wo, in Vector wi) =>
            IsInSameHemisphere(wo, wi) ? AbsCosTheta(wi) * InvPi : 0f;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyFlag(this IBxDF bxdf, BxDFType flag) => (bxdf.Type & flag) != BxDFType.None;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MatchesFlags(this IBxDF bxdf, BxDFType flag) => (bxdf.Type & flag) == bxdf.Type;

        public static Spectrum CosineSampleHemisphereF(this IBxDF bxdf,
                                                       in Vector wo,
                                                       ref Vector wi,
                                                       in Point2D u,
                                                       out float pdf)
        {
            wi = CosineSampleHemisphere(u);
            if (wo.Z < 0f)
            {
                wi = new Vector(wi.X, wi.Y, -wi.Z);
            }

            pdf = bxdf.Pdf(wo, wi);
            return bxdf.F(wo, wi);
        }
    }
}