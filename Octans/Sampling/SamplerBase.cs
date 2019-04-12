using System.Collections.Generic;
using System.Diagnostics;

namespace Octans.Sampling
{
    public abstract class SamplerBase
    {
        protected long CurrentPixelSampleIndex { get; private set; }
        protected PixelCoordinate CurrentPixel { get; private set; }
        private int _array1DIndex;
        private int _array2DIndex;
        private readonly List<float[]> _samples1D = new List<float[]>();
        private readonly List<Point2D[]> _samples2D = new List<Point2D[]>();

        protected SamplerBase(long samplesPerPixel)
        {
            SamplesPerPixel = samplesPerPixel;
        }

        protected IList<float[]> Samples1D => _samples1D;

        protected IList<Point2D[]> Samples2D => _samples2D;

        public long SamplesPerPixel { get; }

        public abstract Point2D Get2D();

        public abstract float Get1D();

        public virtual void StartPixel(in PixelCoordinate p)
        {
            CurrentPixel = p;
            CurrentPixelSampleIndex = 0;
            Reset();
        }

        protected void Reset()
        {
            _array1DIndex = _array2DIndex = 0;
        }

        public virtual int RoundCount(int n) => n;


        public float[] Get1DArray(int n)
        {
            if (_array1DIndex == _samples1D.Count)
            {
                return null;
            }

            var array = _samples1D[_array1DIndex++];
            Debug.Assert(array.Length == n* SamplesPerPixel);
            Debug.Assert(CurrentPixelSampleIndex < SamplesPerPixel);
            //if (array.Length != n*SamplesPerPixel)
            //{
            //    throw new InvalidOperationException($"Array does not have expected length of {n}.");
            //}

            return array;
        }

        public Point2D[] Get2DArray(int n)
        {
            if (_array2DIndex == _samples2D.Count)
            {
                return null;
            }

            var array = _samples2D[_array2DIndex++];
            Debug.Assert(array.Length == n* SamplesPerPixel);
            Debug.Assert(CurrentPixelSampleIndex < SamplesPerPixel);
            //if (array.Length != n * SamplesPerPixel)
            //{
            //    throw new InvalidOperationException($"Array does not have expected length of {n}.");
            //}

            return array;
        }

        public virtual bool StartNextSampler()
        {
            Reset();
            return ++CurrentPixelSampleIndex < SamplesPerPixel;
        }

        public abstract SamplerBase Clone(int seed);

        public virtual bool SetSampleNumber(long sampleNumber)
        {
            Reset();
            CurrentPixelSampleIndex = sampleNumber;
            return CurrentPixelSampleIndex < SamplesPerPixel;
        }

        public CameraSample GetCameraSample(in PixelCoordinate raster)
        {
            var cs = new CameraSample
            {
                FilmPoint = Get2D() + (Point2D) raster,
                LensPoint = Get2D()
            };
            return cs;
        }

        public void Request1DArray(int n)
        {
            Debug.Assert(RoundCount(n)==n);
            _samples1D.Add(new float[n * SamplesPerPixel]);
        }

        public void Request2DArray(int n)
        {
            Debug.Assert(RoundCount(n) == n);
            _samples2D.Add(new Point2D[n * SamplesPerPixel]);
        }
    }
}