using System;
using System.Collections.Generic;
using static System.MathF;
using static Octans.MathF;

namespace Octans
{
    public class SpectralInformation 
    {
        public int Samples => 60;
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
                var wl0 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float)i / Samples);
                var wl1 = Lerp(SampledLambdaStart, SampledLambdaEnd, (float)(i + 1) / Samples);
                c[i] = AverageSpectrumSamples(CIE_Data.RGB2SpectLambda, rgbSpace, n, wl0, wl1);
            }
            return c;
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