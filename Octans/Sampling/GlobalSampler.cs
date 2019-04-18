using System.Diagnostics;

namespace Octans.Sampling
{
    public abstract class GlobalSampler : SamplerBase
    {
        private const int ArrayStartDim = 5;
        private int _arrayEndDim;
        private int _dimension;
        private long _intervalSampleIndex;

        protected GlobalSampler(long samplesPerPixel) : base(samplesPerPixel)
        {
        }

        protected virtual long GetIndexForSample(long sampleNumber) => 0;

        protected virtual float SampleDimension(long index, int dimension) => 0f;

        public override void StartPixel(in PixelCoordinate p)
        {
            base.StartPixel(in p);
            _dimension = 0;
            _intervalSampleIndex = GetIndexForSample(0);
            _arrayEndDim = ComputeArrayEndDimension();
            Compute1DSamples();
            Compute2DSamples();
        }

        public override bool StartNextSample()
        {
            _dimension = 0;
            _intervalSampleIndex = GetIndexForSample(CurrentPixelSampleIndex + 1);
            return base.StartNextSample();
        }

        public override bool SetSampleNumber(long sampleNumber)
        {
            _dimension = 0;
            _intervalSampleIndex = GetIndexForSample(sampleNumber);
            return base.SetSampleNumber(sampleNumber);
        }

        private void Compute2DSamples()
        {
            var dim = ArrayStartDim + Samples1D.Count;
            for (var i = 0; i < Samples2D.Count; i++)
            {
                var array = Samples2D[i];
                for (var j = 0; j < array.Length; j++)
                {
                    var index = GetIndexForSample(j);
                    var x = SampleDimension(index, dim);
                    var y = SampleDimension(index, dim + 1);
                    array[j] = new Point2D(x, y);
                }

                dim += 2;
            }

            Debug.Assert(_arrayEndDim == dim);
        }

        private void Compute1DSamples()
        {
            for (var i = 0; i < Samples1D.Count; i++)
            {
                var array = Samples1D[i];
                for (var j = 0; j < array.Length; j++)
                {
                    var index = GetIndexForSample(j);
                    array[j] = SampleDimension(index, ArrayStartDim + i);
                }
            }
        }

        private int ComputeArrayEndDimension() => ArrayStartDim + Samples1D.Count + 2 * Samples2D.Count;

        public override float Get1D()
        {
            if (_dimension >= ArrayStartDim && _dimension < _arrayEndDim)
            {
                _dimension = _arrayEndDim;
            }

            return SampleDimension(_intervalSampleIndex, _dimension++);
        }

        public override Point2D Get2D()
        {
            if (_dimension + 1 >= ArrayStartDim && _dimension < _arrayEndDim)
            {
                _dimension = _arrayEndDim;
            }

            var x = SampleDimension(_intervalSampleIndex, _dimension);
            var y = SampleDimension(_intervalSampleIndex, _dimension + 1);
            _dimension += 2;
            return new Point2D(x, y);
        }
    }
}