using System;
using System.Threading;

namespace Octans.Sampling
{
    public sealed class HaltonSampler : GlobalSampler
    {
        private const int KMaxResolution = 128;

        private static readonly Lazy<ushort[]> LazyPermutations = new Lazy<ushort[]>(
            () => QuasiRandom.ComputeRadicalInversePermutations(new Random(63)),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private PixelCoordinate _baseExponents;
        private PixelCoordinate _baseScales;
        private long[] _multiplicativeInv = new long[2];
        private int _sampleStride;
        private long _offsetForCurrentPixel;
        private PixelCoordinate _pixelForOffset = new PixelCoordinate(int.MaxValue, int.MinValue);

        public HaltonSampler(int samplesPerPixel, in PixelArea sampleBounds) : base(samplesPerPixel)
        {
            SampleBounds = sampleBounds;

            FindScalesAndExponentsForArea(in sampleBounds, out _baseScales, out _baseExponents);
            _sampleStride = Stride(in _baseScales);
            _multiplicativeInv[0] = MultiplicativeInverse(_baseScales.Y, _baseScales.X);
            _multiplicativeInv[1] = MultiplicativeInverse(_baseScales.X, _baseScales.Y);
        }

        //private HaltonSampler(in HaltonSampler other) : base(other.SamplesPerPixel)
        //{
        //    SampleBounds = other.SampleBounds;
        //    _baseScales = other._baseScales;
        //    _baseExponents = other._baseExponents;
        //    _sampleStride = other._sampleStride;
        //    _multiplicativeInv = other._multiplicativeInv;

        //    foreach (var row in other.Samples1D)
        //    {
        //        Samples1D.Add(new float[row.Length]);
        //    }

        //    foreach (var row in other.Samples2D)
        //    {
        //        Samples2D.Add(new Point2D[row.Length]);
        //    }
        //}

        public HaltonSampler() : base(0)
        { }

        private HaltonSampler Initialize(in HaltonSampler other)
        {
            base.SamplesPerPixel = other.SamplesPerPixel;
            SampleBounds = other.SampleBounds;
            _baseScales = other._baseScales;
            _baseExponents = other._baseExponents;
            _sampleStride = other._sampleStride;
            _multiplicativeInv = other._multiplicativeInv;

            if (Samples1D.Count != other.Samples1D.Count)
            {
                foreach (var row in other.Samples1D)
                {
                    Samples1D.Add(new float[row.Length]);
                }
            }

            if (Samples2D.Count != other.Samples2D.Count)
            {
                foreach (var row in other.Samples2D)
                {
                    Samples2D.Add(new Point2D[row.Length]);
                }
            }

            return this;
        }

        public PixelArea SampleBounds { get; private set; }

        private static ushort[] RadicalInversePermutations => LazyPermutations.Value;

        private static long MultiplicativeInverse(int a, int n)
        {
            var (x, _) = ExtendedGCD(a, n);
            return x % n;
        }

        private static (long x, long y) ExtendedGCD(int a, int b)
        {
            if (b == 0)
            {
                return (1, 0);
            }

            long d = a / b;
            var (xp, yp) = ExtendedGCD(b, a % b);
            return (yp, xp - d * yp);
        }

        private static void FindScalesAndExponentsForArea(in PixelArea sampleBounds,
                                                          out PixelCoordinate baseScales,
                                                          out PixelCoordinate baseExponents)
        {
            baseScales = new PixelCoordinate();
            baseExponents = new PixelCoordinate();
            var res = sampleBounds.Max - sampleBounds.Min;
            for (var i = 0; i < 2; i++)
            {
                var b = i == 0 ? 2 : 3;
                var scale = 1;
                var exp = 0;
                while (scale < System.Math.Min(res[i], KMaxResolution))
                {
                    scale *= b;
                    ++exp;
                }

                if (i == 0)
                {
                    baseScales = baseScales.SetX(scale);
                    baseExponents = baseExponents.SetX(exp);
                }
                else
                {
                    baseScales = baseScales.SetY(scale);
                    baseExponents = baseExponents.SetY(exp);
                }
            }
        }

        protected override long GetIndexForSample(long sampleNumber)
        {
            if (CurrentPixel == _pixelForOffset)
            {
                return _offsetForCurrentPixel + sampleNumber * _sampleStride;
            }

            _offsetForCurrentPixel = ComputeHaltonSampleOffset(CurrentPixel);
            _pixelForOffset = CurrentPixel;

            return _offsetForCurrentPixel + sampleNumber * _sampleStride;
        }

        private long ComputeHaltonSampleOffset(in PixelCoordinate pixel)
        {
            var offset = 0L;
            if (_sampleStride <= 1)
            {
                return offset;
            }

            var pm = new PixelCoordinate(pixel.X % KMaxResolution, pixel.Y % KMaxResolution);
            for (var i = 0; i < 2; i++)
            {
                var dimOffset = i == 0
                    ? InverseRadicalInverse(2, pm.X, _baseExponents.X)
                    : InverseRadicalInverse(3, pm.Y, _baseExponents.Y);
                offset += dimOffset * (_sampleStride / _baseScales[i]) * _multiplicativeInv[i];
            }

            offset %= _sampleStride;

            return offset;
        }

        private static long InverseRadicalInverse(int b, long inverse, int n)
        {
            var index = 0L;
            for (var i = 0; i < n; i++)
            {
                var digit = inverse % b;
                inverse /= b;
                index = index * b + digit;
            }

            return index;
        }

        private static int Stride(in PixelCoordinate baseScales) => baseScales.X * baseScales.Y;

        protected override float SampleDimension(long index, int dimension)
        {
            switch (dimension)
            {
                case 0:
                    return QuasiRandom.RadicalInverseBase2((ulong) (index >> _baseExponents.X));
                case 1:
                    return QuasiRandom.RadicalInverse(1, (ulong) (index / _baseExponents.Y));
                default:
                    var permutations = QuasiRandom.PermutationsForDimension(RadicalInversePermutations, dimension);
                    return QuasiRandom.RadicalInverseScrambled(dimension, (ulong) index, permutations);
            }
        }

        public override ISampler Clone(int seed, IObjectArena arena)
        {
            return arena.Create<HaltonSampler>().Initialize(this);
            //return new HaltonSampler(this);
        }
    }
}