using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public static class Utilities
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagged(this ILight light, LightType type)
        {
            return light.Type == type;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeltaLight(this ILight light)
        {
            return light.Type == LightType.DeltaPosition || 
                   light.Type == LightType.DeltaDirection;
        }

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
    }
}