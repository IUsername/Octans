using System;
using System.Numerics;
using FluentAssertions;
using Octans.Filter;
using Xunit;

namespace Octans.Test
{
    public class FilmTests
    {
        [Fact]
        public void SampleBoundsDependsOnFilterRadius()
        {
            var film = new Film(new PixelVector(800, 600),
                                new Bounds2D(new Point2D(0, 0), new Point2D(1, 1)),
                                new MitchellFilter(new Vector2(2, 3), 0.25f, 0.5f), 10f, 1f);
            var sb = film.GetSampleBounds();
            sb.Min.Should().Be(new PixelCoordinate(-2, -3));
            sb.Max.Should().Be(new PixelCoordinate(802, 603));
        }

        [Fact]
        public void SensorPhysicalExtent()
        {
            var width = 800;
            var height = 600;
            var sensorDiagMillimeters = 10f;
            var film = new Film(new PixelVector(width, height),
                                new Bounds2D(new Point2D(0, 0), new Point2D(1, 1)),
                                new MitchellFilter(new Vector2(2, 3), 0.25f, 0.5f), sensorDiagMillimeters, 1f);
            var ext = film.GetPhysicalExtent();
            var aspect = (float) height / width;
            var meterDiag = sensorDiagMillimeters * 0.001f;
            var x = System.MathF.Sqrt(meterDiag * meterDiag / (1 + aspect * aspect));
            ext.Min.X.Should().BeApproximately(-x / 2f, 0.0001f);
            ext.Min.Y.Should().BeApproximately(-(x * aspect) / 2f, 0.0001f);
        }

        [Fact]
        public void CanCreateFilmTile()
        {
            var width = 800;
            var height = 600;
            var film = new Film(new PixelVector(width, height),
                                new Bounds2D(new Point2D(0, 0), new Point2D(1, 1)),
                                new MitchellFilter(new Vector2(2, 3), 0.25f, 0.5f), 10f, 1f);
            var tileArea = new PixelArea(0,0,200,200);
            var tile = film.CreateFilmTile(in tileArea);
            tile.PixelBounds.Equals(new PixelArea(0, 0, 201, 202)).Should().BeTrue();
        }

        [Fact]
        public void CanAddSamplesToFilmTile()
        {
            var width = 800;
            var height = 600;
            var film = new Film(new PixelVector(width, height),
                                new Bounds2D(0, 0, 1, 1),
                                new MitchellFilter(new Vector2(2, 2), 0.25f, 0.5f), 10f, 1f);
            var testSink = new TestRGBSink();
            film.SetSink(testSink);
            var tileArea = new PixelArea(0, 0, 16, 16);
            var tile = film.CreateFilmTile(in tileArea);
            var l = new Spectrum(1f);
            
            tile.AddSample(new Point2D(0.5f,0.5f), l);
            film.MergeFilmTile(tile);
            film.WriteFile(1f);
            film.Return();

            testSink.Data[0].Should().BeGreaterThan(0f);
            testSink.Data[1].Should().BeGreaterThan(0f);
            testSink.Data[2].Should().BeGreaterThan(0f);

            testSink.Data[800*3+0].Should().BeGreaterThan(0f);
            testSink.Data[800 * 3 + 1].Should().BeGreaterThan(0f);
            testSink.Data[800 * 3 + 2].Should().BeGreaterThan(0f);
        }
    }

    public class TestRGBSink : ISinkRgb
    {
        public float[] Data { get; private set; }
        public PixelArea Area { get; private set; }
        public PixelVector FullResolution { get; private set; }

        public void Write(in ReadOnlySpan<float> rgb, in PixelArea area, in PixelVector fullResolution)
        {
            var length = area.Area();
            var data = new float[length];
            for (var i = 0; i < length; i++)
            {
                data[i] = rgb[i];
            }

            Data = data;
            Area = area;
            FullResolution = fullResolution;
        }
    }
}