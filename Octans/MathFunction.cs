using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public static class MathFunction
    {
        [Pure]
        public static float ClampF(float min, float max, float value)
        {
            return MathF.Min(max, MathF.Max(min, value));
        }

        [Pure]
        public static float MixF(float i, float j, float t)
        {
            return j * t + i * (1f - t);
        }

        /// <summary>
        /// Returns value limited to [0,1]/
        /// </summary>
        /// <param name="value">Value to limit.</param>
        /// <returns>Limited value.</returns>
        [Pure]
        public static float Saturate(float value)
        {
            return MathF.Max(0f, MathF.Min(1f, value));
        }
    }
}