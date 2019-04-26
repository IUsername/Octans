using System;
using static System.MathF;

namespace Octans
{
    internal static class Check
    {
        public static bool Within(float a, float b, float epsilon) =>
            Abs(a - b) < epsilon
            || float.IsPositiveInfinity(a) && float.IsPositiveInfinity(b)
            || float.IsNegativeInfinity(a) && float.IsNegativeInfinity(b);
    }
}