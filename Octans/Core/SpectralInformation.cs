using System;
using System.Collections.Generic;
using static System.MathF;
using static Octans.MathF;

namespace Octans
{
    public class SpectralInformation
    {
        public int Samples => 24;
        public int SampledLambdaEnd => 700;
        public int SampledLambdaStart => 400;

        public (float[] X, float[] Y, float[] Z) GetXYZ()
        {
            var x = new float[Samples];
            var y = new float[Samples];
            var z = new float[Samples];
            var n = CIE_Data.CIE_lambda.Length;
            for (var i = 0; i < Samples; i++)
            {
                var wl0 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float) i / Samples);
                var wl1 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float) (i + 1) / Samples);
                x[i] = AverageSpectrumSamples(CIE_Data.CIE_lambda, CIE_Data.CIE_X, n, wl0, wl1);
                y[i] = AverageSpectrumSamples(CIE_Data.CIE_lambda, CIE_Data.CIE_Y, n, wl0, wl1);
                z[i] = AverageSpectrumSamples(CIE_Data.CIE_lambda, CIE_Data.CIE_Z, n, wl0, wl1);
            }

            return (x, y, z);
        }
      
        public float[] GetRGBSpectrum(float[] rgbSpace)
        {
            var n = CIE_Data.RGB2SpectLambda.Length;
            var c = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                var wl0 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float) i / Samples);
                var wl1 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float) (i + 1) / Samples);
                c[i] = AverageSpectrumSamples(CIE_Data.RGB2SpectLambda, rgbSpace, n, wl0, wl1);
            }

            return c;
        }

        public float[] FromSampled(float[] lambda, float[] v)
        {
            if (!SpectrumSamplesSorted(lambda))
            {
                SortSpectrumSamples(lambda, v);
                return FromSampled(lambda, v);
            }

            var n = lambda.Length;
            var r = new float[Samples];
            for (var i = 0; i < Samples; ++i)
            {
                var wl0 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float)i / Samples);
                var wl1 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float)(i + 1) / Samples);
                r[i] = AverageSpectrumSamples(lambda, v, n, wl0, wl1);
            }

            return r;
        }

        public float[] FromBlackbodyT(float T)
        {
            var v = GetLe(T);
            return FromSampled(CIE_Data.CIE_lambda, v);
        }

        private static void SortSpectrumSamples(float[] lambda, float[] v)
        {
            if (lambda.Length != v.Length)
            {
                throw new InvalidOperationException("Lambda and value arrays must be the same length.");
            }
            Array.Sort(v, lambda);
            Array.Sort(lambda);
        }

        private static bool SpectrumSamplesSorted(float[] lambda)
        {
            for (var i = 0; i < lambda.Length - 1; ++i)
            {
                if (lambda[i] > lambda[i + 1]) return false;
            }

            return true;
        }

        public float[] GetLe(float T)
        {
            BlackbodyNormalized(CIE_Data.CIE_lambda, T, out var Le);
            return Le;
        }

        private static void Blackbody(float[] lambda, float T, out float[] Le)
        {
            var n = lambda.Length;
            Le = new float[n];
            if (T <= 0f)
            {
                for (var i = 0; i < n; ++i)
                {
                    Le[i] = 0f;
                }

                return;
            }

            const float c = 299792458f;
            const float h = 6.62606957e-34f;
            const float kb = 1.3806488e-23f;

            for (var i = 0; i < n; ++i)
            {
                var l = lambda[i] * 1e-9f;
                var lambda5 = (l * l) * (l * l) * l;
                var a = 2f * h * c * c;
                var e = Exp((h * c) / (l * kb * T));
                var b = lambda5 *( e - 1f);
                Le[i] = a / b;
            }
        }

        private static void BlackbodyNormalized(float[] lambda, float T, out float[] Le)
        {
            Blackbody(lambda, T, out Le);
            var lambdaMax = 2.8977721e-3f / T * 1e9f;
            Blackbody(new[] {lambdaMax}, T, out var maxL);
            var m = maxL[0];
            for (var i = 0; i < Le.Length; ++i)
            {
                Le[i] /= m;
            }
        }

        private static float AverageSpectrumSamples(
            IReadOnlyList<float> lambda,
            IReadOnlyList<float> values,
            in int n,
            in float lambdaStart,
            in float lambdaEnd)
        {
            if (lambdaEnd <= lambda[0])
            {
                return values[0];
            }

            if (lambdaStart >= lambda[n - 1])
            {
                return values[n - 1];
            }

            if (n == 1)
            {
                return values[0];
            }

            var sum = 0f;
            if (lambdaStart < lambda[0])
            {
                sum += values[0] * (lambda[0] - lambdaStart);
            }

            if (lambdaEnd > lambda[n - 1])
            {
                sum += values[n - 1] * (lambdaEnd - lambda[n - 1]);
            }

            var i = 0;
            while (lambdaStart > lambda[i + 1]) ++i;

            float Interpolate(float w, int index) =>
                Lerp(values[index], values[index + 1], (w - lambda[index]) / lambda[index + 1]);

            for (; i + 1 < n && lambdaEnd >= lambda[i]; ++i)
            {
                var segStart = Max(lambdaStart, lambda[i]);
                var segEnd = Min(lambdaEnd, lambda[i + 1]);
                sum += 0.5f * (Interpolate(segStart, i) + Interpolate(segEnd, i)) * (segEnd - segStart);
            }

            return sum / (lambdaEnd - lambdaStart);
        }
    }
}