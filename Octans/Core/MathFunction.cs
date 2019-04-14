using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public static class MathFunction
    {
        [Pure]
        public static float ClampF(float min, float max, float value) => MathF.Min(max, MathF.Max(min, value));

        /// <summary>
        /// Linearly interpolates between v0 and v1 by t.
        /// </summary>
        /// <param name="v0">Value at t = 0.</param>
        /// <param name="v1">Value at t = 1.</param>
        /// <param name="t">Point in [0..1].</param>
        /// <returns>Interpolated result.</returns>
        [Pure]
        public static float Lerp(float v0, float v1, float t)
        {
            //  (1f - t) * v0 + t * v1;
            return MathF.FusedMultiplyAdd(t, v1, MathF.FusedMultiplyAdd(-t, v0, v0));
        }

        /// <summary>
        ///     Returns value limited to [0,1]/
        /// </summary>
        /// <param name="value">Value to limit.</param>
        /// <returns>Limited value.</returns>
        [Pure]
        public static float Saturate(float value) => MathF.Max(0f, MathF.Min(1f, value));

        /// <summary>
        /// Returns two vectors that when combined with the input normal can be used to transform to and from a
        /// Z-positive normal orthonormal space.
        /// </summary>
        /// <param name="n">Normal vector.</param>
        /// <returns>Orthonormal vectors from input normal.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Vector b1, Vector b2) OrthonormalPosZ(in Normal n)
        {
            // https://graphics.pixar.com/library/OrthonormalB/paper.pdf
            var sign = MathF.CopySign(1f, n.Z);
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
        public static float Rad(float degrees)
        {
            const float piDiv180 = MathF.PI / 180f;
            return degrees * piDiv180;
        }

        [Pure]
        public static float Deg(float radians)
        {
            const float oneEightyOverPi = 180f / MathF.PI;
            return radians * oneEightyOverPi;
        }

        public static readonly float OneMinusEpsilon = MathF.BitDecrement(1f); // 0.99999994F;
    }
}