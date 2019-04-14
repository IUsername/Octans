using System;
using System.Buffers;
using System.Numerics;
using Octans.Filter;

namespace Octans
{
    public class Film : IDisposable
    {
        private const int FilterTableWidth = 16;
        private readonly PixelArea _croppedBounds;
        private readonly float _diagonalMeters;
        private readonly IFilter _filter;
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
            _croppedBounds = DetermineCroppedBounds(in cropWindow, in resolution);
            _pixels = ArrayPool<Pixel>.Shared.Rent(_croppedBounds.Area());
            _table = CreateFilterTable(FilterTableWidth, filter);
        }

        public void Dispose()
        {
            Return();
        }

        private static PixelArea DetermineCroppedBounds(in Bounds2D cropWindow, in PixelVector resolution) =>
            new PixelArea(
                new PixelCoordinate(
                    (int) MathF.Ceiling(resolution.X * cropWindow.Min.X),
                    (int) MathF.Ceiling(resolution.Y * cropWindow.Min.Y)),
                new PixelCoordinate(
                    (int) MathF.Ceiling(resolution.X * cropWindow.Max.X),
                    (int) MathF.Ceiling(resolution.Y * cropWindow.Max.Y)));

        public FilmTile CreateFilmTile(in PixelArea area)
        {
            var halfPixel = new Vector2(0.5f, 0.5f);
            var bounds = (Bounds2D) area;
            var p0 = (PixelCoordinate) Point2D.Ceiling(bounds.Min - halfPixel - _filter.Radius);
            var p1 = (PixelCoordinate) Point2D.Floor(bounds.Max - halfPixel + _filter.Radius);
            var b = new PixelArea(p0, p1);
            var tilePixelBounds = PixelArea.Intersect(b, _croppedBounds);
            var pixels = ArrayPool<TilePixel>.Shared.Rent(tilePixelBounds.Area());
            return new FilmTile(tilePixelBounds, _filter.Radius, _table, FilterTableWidth, pixels);
        }

        public PixelArea GetSampleBounds()
        {
            var min = (Point2D) _croppedBounds.Min + new Vector2(0.5f, 0.5f) - _filter.Radius;
            var max = (Point2D) _croppedBounds.Max - new Vector2(0.5f, 0.5f) + _filter.Radius;
            var bounds = new Bounds2D(min, max);
            return (PixelArea) bounds;
        }

        public Bounds2D GetPhysicalExtent()
        {
            var aspect = (float) _resolution.Y / _resolution.X;
            var x = MathF.Sqrt(_diagonalMeters * _diagonalMeters / (1 + aspect * aspect));
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

        internal unsafe struct Pixel
        {
            public fixed float XYZ[3];
            public float FilterWeightSum { get; set; }
            public fixed float SplatXYZ[3];
            private float Pad;
        }

        internal struct TilePixel
        {
            //TODO: Spectrum data
            public float FilterWeightSum { get; set; }
        }

        public class FilmTile
        {
            private readonly Vector2 _filterRadius;
            private readonly float[] _filterTable;
            private readonly int _filterTableWidth;
            private readonly Vector2 _inverseFilterRadius;
            private readonly TilePixel[] _pixels;


            internal FilmTile(in PixelArea tileBounds,
                              Vector2 filterRadius,
                              float[] filterTable,
                              int filterTableWidth,
                              TilePixel[] pixels)
            {
                _filterTable = filterTable;
                _filterTableWidth = filterTableWidth;
                _pixels = pixels;
                TileBounds = tileBounds;
                _filterRadius = filterRadius;
                _inverseFilterRadius = new Vector2(1f / filterRadius.X, 1f / filterRadius.Y);
            }

            public PixelArea TileBounds { get; }

           // public void AddSample(in Point2D p, )
        }
    }
}