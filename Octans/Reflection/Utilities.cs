using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;
using static Octans.MathF;
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

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CosTheta(in Vector w) => w.Z;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos2Theta(in Vector w) => w.Z * w.Z;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsCosTheta(in Vector w) => Abs(w.Z);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin2Theta(in Vector w) => Max(0f, 1f - Cos2Theta(in w));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SinTheta(in Vector w) => Sqrt(Sin2Theta(in w));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float TanTheta(in Vector w) => SinTheta(in w) / CosTheta(in w);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan2Theta(in Vector w) => Sin2Theta(in w) / Cos2Theta(in w);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CosPhi(in Vector w)
        {
            var sinTheta = SinTheta(in w);
            return (sinTheta == 0f) ? 1f : Clamp(-1, 1, w.X / sinTheta);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SinPhi(in Vector w)
        {
            var sinTheta = SinTheta(in w);
            return (sinTheta == 0f) ? 0f : Clamp(-1, 1, w.Y / sinTheta);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos2Phi(in Vector w) => CosPhi(in w) * CosPhi(in w);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin2Phi(in Vector w) => SinPhi(in w) * SinPhi(in w);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CosDPhi(in Vector wa, in Vector wb)
        {
            var v = (wa.X * wb.X + wa.Y * wb.Y) / Sqrt((wa.X * wa.X + wa.Y * wa.Y) * (wb.X * wb.X + wb.Y * wb.Y));
            return Clamp(-1, 1, v);
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
    }
}