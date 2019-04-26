using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;

namespace Octans
{
    public static class MathF
    {
        public const float InvPi = 1f / PI;

        public const float Inv2Pi = 1f / (2f * PI);

        public const float Inv4Pi = 1f / (4f * PI);

        public const float PiOver2 = PI / 2f;

        public const float PiOver4 = PI / 4f;

        public const float TwoPi = PI * 2f;

        private const float OneEightyOverPi = 180f / PI;

        private const float PiOver180 = PI / 180f;

        public const float ShadowEpsilon = 0.0001f;

        public static readonly float OneMinusEpsilon = BitDecrement(1f); // 0.99999994F;

        public static readonly float MachineEpsilon;

        public static readonly float SqrtPiInv = 1f / Sqrt(PI);

        static MathF()
        {
            var eps = 1f;
            while (1f + 0.5 * eps != 1f)
            {
                eps = 0.5f * eps;
            }

            MachineEpsilon = eps;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Gamma(int n) => n * MachineEpsilon / (1 - n * MachineEpsilon);

        // TODO: Duplicate in Vector
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsDot(in Vector a, in Vector b) => Abs(Vector.Dot(in a, in b));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsDot(in Normal a, in Vector b) => Abs(Vector.Dot(in a, in b));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float min, float max, float value) => Min(max, Max(min, value));

        /// <summary>
        ///     Linearly interpolates between v0 and v1 by t.
        /// </summary>
        /// <param name="v0">Value at t = 0.</param>
        /// <param name="v1">Value at t = 1.</param>
        /// <param name="t">Point in [0..1].</param>
        /// <returns>Interpolated result.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float v0, float v1, float t) => FusedMultiplyAdd(t, v1, FusedMultiplyAdd(-t, v0, v0));

        /// <summary>
        ///     Returns value limited to [0,1]/
        /// </summary>
        /// <param name="value">Value to limit.</param>
        /// <returns>Limited value.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Saturate(float value) => Max(0f, Min(1f, value));

        /// <summary>
        ///     Returns two vectors that when combined with the input normal can be used to transform to and from a
        ///     Z-positive normal orthonormal space.
        /// </summary>
        /// <param name="n">Normal vector.</param>
        /// <returns>Orthonormal vectors from input normal.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector b1, Vector b2) OrthonormalPosZ(in Normal n)
        {
            // https://graphics.pixar.com/library/OrthonormalB/paper.pdf
            var sign = CopySign(1f, n.Z);
            var aT = -1f / (sign + n.Z);
            var bT = n.X * n.Y * aT;
            var b1T = new Vector(1f + sign * n.X * n.X * aT, sign * bT, -sign * n.X);
            var b2T = new Vector(bT, sign + n.Y * n.Y * aT, -n.Y);
            return (b1T, b2T);

            //if (n.Z < 0f)
            //{
            //    var a = 1f / (1f - n.Z);
            //    var b = n.X * n.Y * a;
            //    var b1 = new Vector(1f - n.X * n.X * a, -b, n.X);
            //    var b2 = new Vector(b, n.Y * n.Y * a - 1f, -n.Y);
            //    return (b1, b2);
            //}
            //else
            //{
            //    var a = 1f / (1f + n.Z);
            //    var b = -n.X * n.Y * a;
            //    var b1 = new Vector(1f - n.X * n.X * a, b, -n.X);
            //    var b2 = new Vector(b, 1f - n.Y * n.Y * a, -n.Y);
            //    return (b1, b2);
            //}
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Rad(float degrees) => degrees * PiOver180;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Deg(float radians) => radians * OneEightyOverPi;

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
            return sinTheta == 0f ? 1f : Clamp(-1, 1, w.X / sinTheta);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SinPhi(in Vector w)
        {
            var sinTheta = SinTheta(in w);
            return sinTheta == 0f ? 0f : Clamp(-1, 1, w.Y / sinTheta);
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

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector SphericalDirection(in float sinTheta, in float cosTheta, in float phi)
        {
            return new Vector(sinTheta * Cos(phi), sinTheta * Sin(phi), cosTheta);
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
            wt = eta * -wi + (eta * cosThetaI - cosThetaT) * (Vector)n;
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
        public static float Erf(in float x)
        {
            const float a1 = 0.254829592f;
            const float a2 = -0.284496736f;
            const float a3 = 1.421413741f;
            const float a4 = -1.453152027f;
            const float a5 = 1.061405429f;
            const float p = 0.3275911f;

            var sign = CopySign(1f, x);
            var xA = Abs(x);

            // A&S formula 7.1.26
            var t = 1f / (1f + p * xA);
            var y = 1 - ((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Exp(-xA * xA);

            return sign * y;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ErfInv(in float x)
        {
            float p;
            var xC = Clamp(x, -.99999f, .99999f);
            var w = -Log((1 - xC) * (1 + xC));
            if (w < 5)
            {
                w -= 2.5f;
                p = 2.81022636e-08f;
                p = 3.43273939e-07f + p * w;
                p = -3.5233877e-06f + p * w;
                p = -4.39150654e-06f + p * w;
                p = 0.00021858087f + p * w;
                p = -0.00125372503f + p * w;
                p = -0.00417768164f + p * w;
                p = 0.246640727f + p * w;
                p = 1.50140941f + p * w;
            }
            else
            {
                w = Sqrt(w) - 3;
                p = -0.000200214257f;
                p = 0.000100950558f + p * w;
                p = 0.00134934322f + p * w;
                p = -0.00367342844f + p * w;
                p = 0.00573950773f + p * w;
                p = -0.0076224613f + p * w;
                p = 0.00943887047f + p * w;
                p = 1.00167406f + p * w;
                p = 2.83297682f + p * w;
            }

            return p * xC;
        }
    }
}