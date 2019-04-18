using System.Numerics;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Sampling
{
    public static class Utilities
    {
        public static Vector UniformSampleHemisphere(in Point2D p)
        {
            var z = p[0];
            var r = Sqrt(Max(0f, 1f - z * z));
            var phi = 2f * PI * p[1];
            return new Vector(r * Cos(phi), r * Sin(phi), z);
        }

        public static float UniformSampleHemispherePdf() => Inv2Pi;

        public static Vector CosineSampleHemisphere(in Point2D p)
        {
            var d = ConcentricSampleDisk(p);
            var z = Sqrt(Max(0f, 1f - d.X * d.X - d.Y * d.Y));
            return new Vector(d.X,d.Y, z);
        }

        private static Point2D ConcentricSampleDisk(in Point2D u)
        {
            var uOffset = 2f * u - new Vector2(1f, 1f);

            if(uOffset.X == 0f && uOffset.Y == 0f) return new Point2D(0f,0f);

            float theta, r;
            if (Abs(uOffset.X) > Abs(uOffset.Y))
            {
                r = uOffset.X;
                theta = PiOver4 * (uOffset.Y / uOffset.X);
            }
            else
            {
                r = uOffset.Y;
                theta = PiOver2 - PiOver4 * (uOffset.X / uOffset.Y);
            }

            return r * new Point2D(Cos(theta), Sin(theta));
        }

        public static float CosineHemispherePdf(float cosTheta)
        {
            return cosTheta * InvPi;
        }
    }
}