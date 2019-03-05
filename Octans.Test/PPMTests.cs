using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PPMTests
    {
        [Fact]
        public void ProperHeader()
        {
            var c = new Canvas(5,3);
            var ppm = PPM.CanvasToPPM(c);
            var lines = ppm.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            lines[0].Should().Be("P3");
            lines[1].Should().Be("5 3");
            lines[2].Should().Be("255");
        }

        [Fact]
        public void PPMPixelData()
        {
            var c = new Canvas(5, 3);
            c.WritePixel(Color.Create(1.5, 0, 0), 0, 0);
            c.WritePixel(Color.Create(0, 0.5, 0), 2, 1);
            c.WritePixel(Color.Create(-0.5, 0, 1), 4, 2);
            var ppm = PPM.CanvasToPPM(c);
            var lines = ppm.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            lines[3].Should().Be("255 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
            lines[4].Should().Be("0 0 0 0 0 0 0 128 0 0 0 0 0 0 0");
            lines[5].Should().Be("0 0 0 0 0 0 0 0 0 0 0 0 0 0 255");
        }

        [Fact]
        public void SplitsLongLines()
        {
            var c = new Canvas(10,2);
            c.SetAllPixels(Color.Create(1,0.8,0.6));
            var ppm = PPM.CanvasToPPM(c);
            var lines = ppm.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            lines[3].Should().Be("255 204 153 255 204 153 255 204 153 255 204 153 255 204 153 255 204");
            lines[4].Should().Be("153 255 204 153 255 204 153 255 204 153 255 204 153");
            lines[5].Should().Be("255 204 153 255 204 153 255 204 153 255 204 153 255 204 153 255 204");
            lines[6].Should().Be("153 255 204 153 255 204 153 255 204 153 255 204 153");
        }

        [Fact]
        public void EndsInNewLineChar()
        {
            var c = new Canvas(10, 2);
            c.SetAllPixels(Color.Create(1, 0.8, 0.6));
            var ppm = PPM.CanvasToPPM(c);
            ppm.Should().EndWith(Environment.NewLine);
        }
    }
}