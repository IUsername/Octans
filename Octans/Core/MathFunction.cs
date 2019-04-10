using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public static class MathFunction
    {
        [Pure]
        public static float ClampF(float min, float max, float value) => MathF.Min(max, MathF.Max(min, value));

        [Pure]
        public static float MixF(float i, float j, float t) => j * t + i * (1f - t);

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
        public static (Vector b1, Vector b2) OrthonormalVectorsPosZ(in Vector n)
        {
            // https://graphics.pixar.com/library/OrthonormalB/paper.pdf
            //var sign = n.Z >= 0f ? 1f : -1f;
            //var a = -1f / (sign + n.Z);
            //var b = n.X * n.Y * a;
            //var b1 = new Vector(1f+sign*n.X*n.X+a, sign*b, -sign*n.X);
            //var b2 = new Vector(b,sign+n.Y*n.Y*a, -n.Y);
            //return (b1, b2);


            if (n.Z < 0f)
            {
                var a = 1f / (1f - n.Z);
                var b = n.X * n.Y * a;
                var b1 = new Vector(1f - n.X * n.X * a, -b, n.X);
                var b2 = new Vector(b, n.Y * n.Y * a - 1f, -n.Y);
                return (b1, b2);
            }
            else
            {
                var a = 1f / (1f + n.Z);
                var b = -n.X * n.Y * a;
                var b1 = new Vector(1f - n.X * n.X * a, b, -n.X);
                var b2 = new Vector(b, 1f - n.Y * n.Y * a, -n.Y);
                return (b1, b2);
            }
        }

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
            //var sign = n.Z >= 0f ? 1f : -1f;
            //var a = -1f / (sign + n.Z);
            //var b = n.X * n.Y * a;
            //var b1 = new Vector(1f+sign*n.X*n.X+a, sign*b, -sign*n.X);
            //var b2 = new Vector(b,sign+n.Y*n.Y*a, -n.Y);
            //return (b1, b2);


            if (n.Z < 0f)
            {
                var a = 1f / (1f - n.Z);
                var b = n.X * n.Y * a;
                var b1 = new Vector(1f - n.X * n.X * a, -b, n.X);
                var b2 = new Vector(b, n.Y * n.Y * a - 1f, -n.Y);
                return (b1, b2);
            }
            else
            {
                var a = 1f / (1f + n.Z);
                var b = -n.X * n.Y * a;
                var b1 = new Vector(1f - n.X * n.X * a, b, -n.X);
                var b2 = new Vector(b, 1f - n.Y * n.Y * a, -n.Y);
                return (b1, b2);
            }
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

       
    }
}