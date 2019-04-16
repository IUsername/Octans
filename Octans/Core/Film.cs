using System;
using System.Buffers;
using System.Numerics;
using Octans.Filter;

namespace Octans
{
    public class Film : IDisposable
    {
        private const int FilterTableWidth = 16;
        private readonly float _diagonalMeters;
        private readonly IFilter _filter;
        private readonly object _padlock = new object();
        private readonly Pixel[] _pixels;
        private readonly PixelVector _resolution;
        private readonly float _scale;
        private readonly float[] _table;
        private bool _returned;

        public Film(in PixelVector resolution,
                    in Bounds2D cropWindow,
                    IFilter filter,
                    float sensorDiagonalMillimeters,
                    float scale)
        {
            _resolution = resolution;
            _filter = filter;
            _scale = scale;
            _diagonalMeters = sensorDiagonalMillimeters * .001f;
            CroppedBounds = DetermineCroppedBounds(in cropWindow, in resolution);
            _pixels = ArrayPool<Pixel>.Shared.Rent(CroppedBounds.Area());
            _table = CreateFilterTable(FilterTableWidth, filter);
        }

        public PixelArea CroppedBounds { get; }

        public void Dispose()
        {
            Return();
        }

        private static PixelArea DetermineCroppedBounds(in Bounds2D cropWindow, in PixelVector resolution) =>
            new PixelArea(
                new PixelCoordinate(
                    (int) System.MathF.Ceiling(resolution.X * cropWindow.Min.X),
                    (int) System.MathF.Ceiling(resolution.Y * cropWindow.Min.Y)),
                new PixelCoordinate(
                    (int) System.MathF.Ceiling(resolution.X * cropWindow.Max.X),
                    (int) System.MathF.Ceiling(resolution.Y * cropWindow.Max.Y)));

        public FilmTile CreateFilmTile(in PixelArea area)
        {
            var halfPixel = new Vector2(0.5f, 0.5f);
            var bounds = (Bounds2D) area;
            var p0 = (PixelCoordinate) Point2D.Ceiling(bounds.Min - halfPixel - _filter.Radius);
            var p1 = (PixelCoordinate) Point2D.Floor(bounds.Max - halfPixel + _filter.Radius);
            var b = new PixelArea(p0, p1);
            var tilePixelBounds = PixelArea.Intersect(b, CroppedBounds);
            return new FilmTile(tilePixelBounds, _filter.Radius, _table, FilterTableWidth);
        }

        public void MergeFilmTile(FilmTile tile)
        {
            lock (_padlock)
            {
                foreach (var pixel in tile.PixelBounds)
                {
                    ref var tilePixel = ref tile.GetPixel(in pixel);
                    ref var mergePixel = ref GetPixel(in pixel);
                    var xyz = tilePixel.ContributionSum.ToXYZ();
                    mergePixel.X += xyz[0];
                    mergePixel.Y += xyz[1];
                    mergePixel.Z += xyz[2];
                    mergePixel.FilterWeightSum += tilePixel.FilterWeightSum;
                }
            }

            tile.Return();
        }

        public void SetImage(in Spectrum[] img)
        {
            var nPixels = CroppedBounds.Area();
            for (var i = 0; i < nPixels; i++)
            {
                ref var p = ref _pixels[i];
                var xyz = img[i].ToXYZ();
                p.X = xyz[0];
                p.Y = xyz[1];
                p.Z = xyz[2];
                p.SplatX = p.SplatY = p.SplatZ = 0f;
                p.FilterWeightSum = 1;
            }
        }

        public void AddSplat(in Point2D p, in Spectrum v)
        {
            var coordinate = (PixelCoordinate) p;
            if (!CroppedBounds.InsideExclusive(in coordinate))
            {
                return;
            }

            var xyz = v.ToXYZ();
            ref var pixel = ref GetPixel(in coordinate);
            pixel.SplatX = xyz[0];
            pixel.SplatY = xyz[1];
            pixel.SplatZ = xyz[2];
        }

        public void WriteFile(float splatScale, ISinkRgb sink)
        {
            var rgb = new float[3 * CroppedBounds.Area()];
            var offset = 0;
            var data = new Span<float>(rgb);
            foreach(var p in CroppedBounds)
            {
                ref var pixel = ref GetPixel(in p);
                var rgbSpan = data.Slice(3 * offset, 3);
                Spectrum.XYZToRGB(pixel.X, pixel.Y, pixel.Z, in rgbSpan);
                var filterWeightSum = pixel.FilterWeightSum;
                if (filterWeightSum != 0f)
                {
                    var invWt = 1f / filterWeightSum;
                    rgbSpan[0] = System.MathF.Max(0f, rgbSpan[0] * invWt);
                    rgbSpan[1] = System.MathF.Max(0f, rgbSpan[1] * invWt);
                    rgbSpan[2] = System.MathF.Max(0f, rgbSpan[2] * invWt);
                }

                var splatRGB = new float[3];
                Spectrum.XYZToRGB(pixel.SplatX, pixel.SplatY, pixel.SplatZ, splatRGB);
                rgbSpan[0] += splatScale * splatRGB[0];
                rgbSpan[1] += splatScale * splatRGB[1];
                rgbSpan[2] += splatScale * splatRGB[2];

                rgbSpan[0] *= _scale;
                rgbSpan[1] *= _scale;
                rgbSpan[2] *= _scale;

                ++offset;
            }
            sink.Write(rgb, CroppedBounds, in _resolution);
        }

        private ref Pixel GetPixel(in PixelCoordinate pixel)
        {
            var width = CroppedBounds.Max.X - CroppedBounds.Min.X;
            var offset = pixel.X - CroppedBounds.Min.X + (pixel.Y - CroppedBounds.Min.Y) * width;
            return ref _pixels[offset];
        }

        public PixelArea GetSampleBounds()
        {
            var min = (Point2D) CroppedBounds.Min + new Vector2(0.5f, 0.5f) - _filter.Radius;
            var max = (Point2D) CroppedBounds.Max - new Vector2(0.5f, 0.5f) + _filter.Radius;
            var bounds = new Bounds2D(min, max);
            return (PixelArea) bounds;
        }

        public Bounds2D GetPhysicalExtent()
        {
            var aspect = (float) _resolution.Y / _resolution.X;
            var x = System.MathF.Sqrt(_diagonalMeters * _diagonalMeters / (1 + aspect * aspect));
            var y = aspect * x;
            return new Bounds2D(new Point2D(-x / 2f, -y / 2f), new Point2D(x / 2f, y / 2f));
        }

        private static float[] CreateFilterTable(int width, IFilter filter)
        {
            var table = new float[width * width];
            var offset = 0;
            for (var y = 0; y < width; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var p = new Point2D(
                        (x + 0.5f) * filter.Radius.X / width,
                        (y + 0.5f) * filter.Radius.Y / width);
                    table[offset++] = filter.Evaluate(in p);
                }
            }

            return table;
        }

        public void Return()
        {
            if (_returned)
            {
                return;
            }

            _returned = true;
            ArrayPool<Pixel>.Shared.Return(_pixels, true);
        }

        internal struct Pixel
        {
            public float X;
            public float Y;
            public float Z;
            public float SplatX;
            public float SplatY;
            public float SplatZ;
            public float FilterWeightSum;
            private readonly float Pad; // Align struct
        }

        internal class TilePixel
        {
            public float FilterWeightSum { get; set; }
            public Spectrum ContributionSum { get; set; }

            public TilePixel()
            {
                FilterWeightSum = 0f;
                ContributionSum = Spectrum.Zero;
            }
        }

        public class FilmTile
        {
            private readonly Vector2 _filterRadius;
            private readonly float[] _filterTable;
            private readonly int _filterTableWidth;
            private readonly Vector2 _inverseFilterRadius;
            private readonly TilePixel[] _pixels;


            internal FilmTile(in PixelArea pixelBounds,
                              in Vector2 filterRadius,
                              float[] filterTable,
                              int filterTableWidth)
            {
                _filterTable = filterTable;
                _filterTableWidth = filterTableWidth;
                PixelBounds = pixelBounds;
                _filterRadius = filterRadius;
                _inverseFilterRadius = new Vector2(1f / filterRadius.X, 1f / filterRadius.Y);
                var p = ArrayPool<TilePixel>.Shared.Rent(pixelBounds.Area());
                for (var i = 0; i < p.Length; i++)
                {
                    p[i] = new TilePixel();
                }

                _pixels = p;
            }

            public PixelArea PixelBounds { get; }

            public void AddSample(in Point2D p, Spectrum L, float sampleWeight = 1f)
            {
                var discrete = p - new Vector2(0.5f, 0.5f);
                var p0 = (PixelCoordinate) Point2D.Ceiling(discrete - _filterRadius);
                var p1 = (PixelCoordinate) Point2D.Floor(discrete + _filterRadius) + new PixelVector(1, 1);
                p0 = PixelCoordinate.Max(p0, PixelBounds.Min);
                p1 = PixelCoordinate.Min(p1, PixelBounds.Max);

                var ifx = new int[p1.X - p0.X];
                for (var x = p0.X; x < p1.X; x++)
                {
                    var fx = System.MathF.Abs((x - discrete.X) * _inverseFilterRadius.X * _filterTableWidth);
                    ifx[x - p0.X] = Math.Min((int) System.MathF.Floor(fx), _filterTableWidth - 1);
                }

                var ify = new int[p1.Y - p0.Y];
                for (var y = p0.Y; y < p1.Y; y++)
                {
                    var fy = System.MathF.Abs((y - discrete.Y) * _inverseFilterRadius.Y * _filterTableWidth);
                    ify[y - p0.Y] = Math.Min((int) System.MathF.Floor(fy), _filterTableWidth - 1);
                }

                for (var y = p0.Y; y < p1.Y; y++)
                {
                    for (var x = p0.X; x < p1.X; x++)
                    {
                        var offset = ify[y - p0.Y] * _filterTableWidth + ifx[x - p0.X];
                        var filterWeight = _filterTable[offset];

                        ref var pixel = ref GetPixel(new PixelCoordinate(x, y));
                        pixel.ContributionSum += L * sampleWeight * filterWeight;
                        pixel.FilterWeightSum += filterWeight;
                    }
                }
            }

            internal ref TilePixel GetPixel(in PixelCoordinate pixel)
            {
                var width = PixelBounds.Max.X - PixelBounds.Min.X;
                var offset = pixel.X - PixelBounds.Min.X + (pixel.Y - PixelBounds.Min.Y) * width;
                return ref _pixels[offset];
            }

            protected internal void Return()
            {
                ArrayPool<TilePixel>.Shared.Return(_pixels);
            }
        }
    }
}