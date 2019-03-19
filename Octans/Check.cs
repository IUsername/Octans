using System;

namespace Octans
{
    internal static class Check
    {
        public static bool Within(float a, float b, float epsilon) =>
            Math.Abs(a - b) < epsilon
            || float.IsPositiveInfinity(a) && float.IsPositiveInfinity(b)
            || float.IsNegativeInfinity(a) && float.IsNegativeInfinity(b);
    }
}