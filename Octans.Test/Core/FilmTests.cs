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
            sb.Min.Should().Be(new PixelCoordinate(-2,-3));
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
            var x = MathF.Sqrt(meterDiag * meterDiag / (1 + aspect*aspect));
            ext.Min.X.Should().BeApproximately(-x / 2f, 0.0001f);
            ext.Min.Y.Should().BeApproximately(-(x * aspect) / 2f, 0.0001f);
        }
    }
}