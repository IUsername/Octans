using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;

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
            return po.PushAway();
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
            if (float.IsInfinity(value) && value > 0f)
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
            if (float.IsInfinity(value) && value < 0f)
            {
                return value;
            }

            if (value == 0f)
            {
                value = -0f;
            }

            return value >= 0f ? BitDecrement(value) : BitIncrement(value);
        }
    }
}