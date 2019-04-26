using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.Math;

namespace Octans
{
    public static class Math
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int min, int max, int value) => Min(max, Max(min, value));
    }
}