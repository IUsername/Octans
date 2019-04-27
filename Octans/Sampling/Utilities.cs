using System;
using System.Diagnostics;
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
    }

    public class Distribution1D
    {
        private readonly float[] _cdf;
        private readonly float[] _func;
        private readonly float _funcInt;

        public Distribution1D(float[] f, int n)
        {
            _func = new float[n];
            Array.Copy(f, _func, n);

            _cdf = new float[n + 1];

            _cdf[0] = 0f;
            for (var i = 1; i < n + 1; ++i)
            {
                _cdf[i] = _cdf[i - 1] + _func[i - 1] / n;
            }

            _funcInt = _cdf[n];
            if (_funcInt == 0f)
            {
                for (var i = 1; i < n + 1; ++i)
                {
                    _cdf[i] = (float) i / n;
                }
            }
            else
            {
                for (var i = 1; i < n + 1; ++i)
                {
                    _cdf[i] /= _funcInt;
                }
            }
        }

        public int Count => _func.Length;

        public float SampleContinuous(float u, out float pdf, out int off)
        {
            var offset = Octans.Utilities.FindInterval(_cdf.Length, i => _cdf[i] <= u);
            off = offset;
            var du = u - _cdf[offset];
            if (_cdf[offset + 1] - _cdf[offset] > 0f)
            {
                du /= _cdf[offset + 1] - _cdf[offset];
            }

            Debug.Assert(!float.IsNaN(du));

            pdf = _funcInt > 0f ? _func[offset] / _funcInt : 0f;

            return (offset + du) / Count;
        }

        public int SampleDiscrete(float u, out float pdf, out float uRemapped)
        {
            var offset = Octans.Utilities.FindInterval(_cdf.Length, i => _cdf[i] <= u);
            pdf = _funcInt > 0f ? _func[offset] / _funcInt : 0f;
            uRemapped = (u - _cdf[offset]) / (_cdf[offset + 1] - _cdf[offset]);
            Debug.Assert(uRemapped >= 0f && uRemapped <= 1f);
            return offset;
        }

        [Pure]
        public float DiscretePdf(int index) => _func[index] / (_funcInt * Count);
    }
}