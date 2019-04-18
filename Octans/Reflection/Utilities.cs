using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;
using static Octans.MathF;
using static Octans.Sampling.Utilities;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Octans.Reflection
{
    public static class Utilities
    {
        public static float FrDielectric(float cosThetaI, float etaI, float etaT)
        {
            cosThetaI = Clamp(-1, 1, cosThetaI);
            var isEntering = cosThetaI > 1f;
            if (!isEntering)
            {
                (etaI, etaT) = (etaT, etaI);
                cosThetaI = Abs(cosThetaI);
            }

            var sinThetaI = Sqrt(Max(0f, 1 - cosThetaI * cosThetaI));
            var sinThetaT = etaI / etaT * sinThetaI;

            if (sinThetaT > 1f)
            {
                // Total internal reflection.
                return 1f;
            }

            var cosThetaT = Sqrt(Max(0f, 1 - sinThetaT * sinThetaT));

            var rParallel = (etaT * cosThetaI - etaI * cosThetaT) / (etaT * cosThetaI + etaI * cosThetaT);
            var rPerpendicular = (etaI * cosThetaI - etaT * cosThetaT) / (etaI * cosThetaI + etaT * cosThetaT);
            return (rParallel * rParallel + rPerpendicular + rPerpendicular) / 2f;
        }

        public static Spectrum FrConductor(float cosThetaI, in Spectrum etaI, in Spectrum etaT, in Spectrum k)
        {
            cosThetaI = Clamp(-1, 1, cosThetaI);
            var eta = etaT / etaI;
            var etaK = k / etaI;

            var cosThetaI2 = cosThetaI * cosThetaI;
            var sinThetaI2 = 1f - cosThetaI2;
            var eta2 = eta * eta;
            var etaK2 = etaK * etaK;

            var t0 = eta2 - etaK2 - sinThetaI2;
            var a2Pb2 = (t0 * t0 + 4f * eta2 * etaK2).Sqrt();
            var t1 = a2Pb2 + cosThetaI2;
            var a = (0.5f * (a2Pb2 + t0)).Sqrt();
            var t2 = 2f * cosThetaI * a;
            var rPerpendicular = (t1 - t2) / (t1 + t2);
            var t3 = cosThetaI2 * a2Pb2 + sinThetaI2 * sinThetaI2;
            var t4 = t2 * sinThetaI2;
            var rParallel = rPerpendicular * (t3 - t4) / (t3 + t4);

            return 0.5f * (rParallel + rPerpendicular);
        }

        // TODO: Duplicate definitions.
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector Reflect(in Vector wo, in Vector n) => -wo + 2f * (wo % n) * n;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Refract(in Vector wi, in Normal n, float eta, ref Vector wt)
        {
            // Snell's Law
            var cosThetaI = n % wi;
            var sin2ThetaI = Max(0f, 1f - cosThetaI * cosThetaI);
            var sin2ThetaT = eta * eta * sin2ThetaI;

            // Total internal reflection
            if (sin2ThetaT >= 1f) return false;

            var cosThetaT = Sqrt(1f - sin2ThetaT);
            wt = eta * -wi + (eta * cosThetaI - cosThetaT) * (Vector) n;
            return true;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInSameHemisphere(in Vector w, in Vector wp) => w.Z * wp.Z > 0f;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInSameHemisphere(in Vector w, in Normal wp) => w.Z * wp.Z > 0f;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoughnessToAlpha(float roughness)
        {
            roughness = Max(roughness, 1e-3f);
            var x = Log(roughness);
            return 1.62142f + 0.819955f * x + 0.1734f * x * x + 0.0171201f * x * x * x + 0.000640711f * x * x * x * x;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Spectrum Rho(IBxDF bxdf, in Vector wo, int nSamples, in Point2D[] u)
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
        public static Spectrum Rho(IBxDF bxdf, int nSamples, in Point2D[] u1, in Point2D[] u2)
        {
            var r = Spectrum.Zero;
            for (var i = 0; i < nSamples; i++)
            {
                var wi = new Vector();
                var wo = UniformSampleHemisphere(u1[i]);
                var pdfo = UniformSampleHemispherePdf();
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
        public static bool IsFlagged(this IBxDF bxdf, BxDFType flag)
        {
            return (bxdf.Type & flag) == flag;
        }
    }
}