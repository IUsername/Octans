using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Octans.Sampling
{
    public interface ISampler
    {
        void StartPixel(in PixelCoordinate p);
        bool StartNextSample();
        ISampler Clone(int seed, IObjectArena arena);
        bool SetSampleNumber(long sampleNumber);
        CameraSample GetCameraSample(in PixelCoordinate raster, IObjectArena arena);
        int RoundCount(int n);
        void Request2DArray(int n);
        Point2D[] Get2DArray(int n);
        long SamplesPerPixel { get; }
        Point2D Get2D();
        float Get1D();
    }

    public abstract class SamplerBase : ISampler
    {
        private readonly List<float[]> _samples1D = new List<float[]>();
        private readonly List<Point2D[]> _samples2D = new List<Point2D[]>();
        private int _array1DIndex;
        private int _array2DIndex;

        protected SamplerBase(long samplesPerPixel)
        {
            SamplesPerPixel = samplesPerPixel;
        }

        protected long CurrentPixelSampleIndex { get; private set; }
        protected PixelCoordinate CurrentPixel { get; private set; }

        protected IList<float[]> Samples1D => _samples1D;

        protected IList<Point2D[]> Samples2D => _samples2D;

        public long SamplesPerPixel { get; protected set; }

        public abstract Point2D Get2D();

        public abstract float Get1D();

        public virtual void StartPixel(in PixelCoordinate p)
        {
            CurrentPixel = p;
            CurrentPixelSampleIndex = 0;
            Reset();
        }

        private void Reset()
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
            Debug.Assert(array.Length == n * SamplesPerPixel);
            Debug.Assert(CurrentPixelSampleIndex < SamplesPerPixel);

            return array;
        }

        public Point2D[] Get2DArray(int n)
        {
            if (_array2DIndex == _samples2D.Count)
            {
                return null;
            }

            var array = _samples2D[_array2DIndex++];
            //Debug.Assert(array.Length == n * SamplesPerPixel);
            Debug.Assert(array.Length == n);
            Debug.Assert(CurrentPixelSampleIndex < SamplesPerPixel);

            return array;
        }

        public virtual bool StartNextSample()
        {
            Reset();
            return ++CurrentPixelSampleIndex < SamplesPerPixel;
        }

        public abstract ISampler Clone(int seed, IObjectArena arena);

        public virtual bool SetSampleNumber(long sampleNumber)
        {
            Reset();
            CurrentPixelSampleIndex = sampleNumber;
            return CurrentPixelSampleIndex < SamplesPerPixel;
        }

        public CameraSample GetCameraSample(in PixelCoordinate raster, IObjectArena arena)
        {
            var cs = arena.Create<CameraSample>();
            cs.FilmPoint = Get2D() + (Vector2) raster;
            cs.LensPoint = Get2D();
            return cs;
        }

        public void Request1DArray(int n)
        {
            Debug.Assert(RoundCount(n) == n);
            //_samples1D.Add(new float[n * SamplesPerPixel]);
            _samples1D.Add(new float[n]);
        }

        public void Request2DArray(int n)
        {
            Debug.Assert(RoundCount(n) == n);
            //_samples2D.Add(new Point2D[n * SamplesPerPixel]);
            _samples2D.Add(new Point2D[n]);
        }
    }
}