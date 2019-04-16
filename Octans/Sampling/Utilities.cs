using static System.MathF;
using static Octans.MathF;

namespace Octans.Sampling
{
    public static class Utilities
    {
        public static Vector UniformSampleHemisphere(in Point p)
        {
            var z = p[0];
            var r = Sqrt(Max(0f, 1f - z * z));
            var phi = 2f * PI * p[1];
            return new Vector(r * Cos(phi), r * Sin(phi), z);
        }

        public static float UniformSampleHemispherePdf() => Inv2Pi;
    }
}