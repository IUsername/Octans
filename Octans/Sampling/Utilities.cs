using System.Diagnostics.Contracts;
using System.Numerics;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Sampling
{
    public static class Utilities
    {
        [Pure]
        public static Vector UniformSampleHemisphere(in Point2D u)
        {
            var z = u[0];
            var r = Sqrt(Max(0f, 1f - z * z));
            var phi = 2f * PI * u[1];
            return new Vector(r * Cos(phi), r * Sin(phi), z);
        }

        [Pure]
        public static float UniformHemispherePdf() => Inv2Pi;

        [Pure]
        public static Vector CosineSampleHemisphere(in Point2D p)
        {
            var d = ConcentricSampleDisk(p);
            var z = Sqrt(Max(0f, 1f - d.X * d.X - d.Y * d.Y));
            return new Vector(d.X, d.Y, z);
        }

        [Pure]
        public static Point2D ConcentricSampleDisk(in Point2D u)
        {
            var uOffset = 2f * u - new Vector2(1f, 1f);

            if (uOffset.X == 0f && uOffset.Y == 0f)
            {
                return new Point2D(0f, 0f);
            }

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

        [Pure]
        public static float CosineHemispherePdf(float cosTheta) => cosTheta * InvPi;

        [Pure]
        public static float UniformSpherePdf() => Inv4Pi;

        [Pure]
        public static Vector UniformSampleSphere(in Point2D u)
        {
            var z = 1f - 2f * u[0];
            var r = Sqrt(Max(0f, 1f - z * z));
            var phi = 2f * PI * u[1];
            return new Vector(r * Cos(phi), r * Sin(phi), z);
        }

        [Pure]
        public static float PowerHeuristic(int nf, float fPdf, int ng, float gPdf)
        {
            var f = nf * fPdf;
            var g = ng * gPdf;
            return f * f / (f * f + g * g);
        }

        [Pure]
        public static float UniformConePdf(in float cosThetaMax)
        {
            return 1f / (2f * PI * (1f - cosThetaMax));
        }

        [Pure]
        public static Vector UniformSampleCone(in Point2D u, in float cosThetaMax)
        {
            var cosTheta = (1f - u[0]) + u[0] * cosThetaMax;
            var sinTheta = Sqrt(1f - cosTheta * cosTheta);
            var phi = u[1] * 2f * PI;
            return new Vector(Cos(phi) * sinTheta, Sin(phi) * sinTheta, cosTheta);
        }

        [Pure]
        public static Point2D UniformSampleTriangle(in Point2D u)
        {
            var su0 = Sqrt(u[0]);
            return new Point2D(1f - su0, u[1] * su0);
        }
    }
}