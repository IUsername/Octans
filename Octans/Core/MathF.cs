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

        private const float OneEightyOverPi = 180f / PI;

        private const float PiOver180 = PI / 180f;

        public static readonly float OneMinusEpsilon = BitDecrement(1f); // 0.99999994F;

        public const float ShadowEpsilon = 0.0001f;

        public static readonly float MachineEpsilon;

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
    }
}