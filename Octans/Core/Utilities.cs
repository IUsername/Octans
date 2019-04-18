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
    }
}