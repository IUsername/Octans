using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Octans.Sampling
{
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