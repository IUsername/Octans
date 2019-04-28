using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;
using static System.Single;

namespace Octans
{
    public static class Utilities
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagged(this ILight light, LightType type) => light.Type == type;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeltaLight(this ILight light) =>
            light.Type == LightType.DeltaPosition ||
            light.Type == LightType.DeltaDirection;

        [Pure]
        public static Point OffsetRayOrigin(in Point p, in Vector pError, in Normal n, in Vector w)
        {
            var d = Normal.Abs(n) % pError;

            var offset = d * (Vector) n;
            if (w % n < 0f)
            {
                offset = -offset;
            }

            var po = p + offset;
            var comp = new float[3];
            for (var i = 0; i < 3; ++i)
            {
                if (offset[i] > 0f)
                {
                    comp[i] = NextFloatUp(po[i]);
                }
                else if (offset[i] < 0f)
                {
                    comp[i] = NextFloatDown(po[i]);
                }
                else
                {
                    comp[i] = po[i];
                }
            }

            return new Point(comp[0], comp[1], comp[2]);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GammaCorrect(in float value)
        {
            if (value <= 0.0031308f)
            {
                return 12.92f * value;
            }

            return 1.055f * Pow(value, 1f / 2.4f) - 0.055f;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InvGammaCorrect(in float value)
        {
            if (value <= 0.04045f)
            {
                return value * 1f / 12.92f;
            }

            return Pow((value + 0.055f) * 1f / 1.055f, 2.4f);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloatUp(float value)
        {
            if (IsPositiveInfinity(value))
            {
                return value;
            }

            if (value == -0f)
            {
                value = 0f;
            }

            return value >= 0f ? BitIncrement(value) : BitDecrement(value);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloatDown(float value)
        {
            if (IsNegativeInfinity(value))
            {
                return value;
            }

            if (value == 0f)
            {
                value = -0f;
            }

            return value >= 0f ? BitDecrement(value) : BitIncrement(value);
        }

        public static bool SolveLinearSystem2x2(float[][] A, float[] B, out float x0, out float x1)
        {
            Debug.Assert(A.Length == 2);
            Debug.Assert(A[0].Length == 2 && A[1].Length == 2);
            Debug.Assert(B.Length == 2);
            var det = A[0][0] * A[1][1] - A[0][1] * A[1][0];
            if (Abs(det) < 1e-10f)
            {
                x0 = 0f;
                x1 = 0f;
                return false;
            }

            x0 = (A[1][1] * B[0] - A[0][1] * B[1]) / det;
            x1 = (A[0][0] * B[1] - A[1][0] * B[0]) / det;
            return !IsNaN(x0) && !IsNaN(x1);
        }

        [Pure]
        public static int FindInterval(int size, Predicate<int> predicate)
        {
            int first = 0;
            var len = size;
            while (len > 0)
            {
                int half = len >> 1;
                var middle = first + half;
                if (predicate(middle))
                {
                    first = middle + 1;
                    len -= half + 1;
                }
                else
                {
                    len = half;
                }
            }

            return Math.Clamp(0, size - 2, first - 1);
        }
    }
}