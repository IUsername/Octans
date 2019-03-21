using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PPMTests
    {
        [Fact]
        public void ProperHeader()
        {
            var c = new Canvas(5, 3);
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
            c.WritePixel(new Color(1.5f, 0f, 0f), 0, 0);
            c.WritePixel(new Color(0f, 0.5f, 0f), 2, 1);
            c.WritePixel(new Color(-0.5f, 0f, 1f), 4, 2);
            var ppm = PPM.CanvasToPPM(c);
            var lines = ppm.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            lines[3].Should().Be("255 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
            lines[4].Should().Be("0 0 0 0 0 0 0 128 0 0 0 0 0 0 0");
            lines[5].Should().Be("0 0 0 0 0 0 0 0 0 0 0 0 0 0 255");
        }

        [Fact]
        public void SplitsLongLines()
        {
            var c = new Canvas(10, 2);
            c.SetAllPixels(new Color(1f, 0.8f, 0.6f));
            var ppm = PPM.CanvasToPPM(c);
            var lines = ppm.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            lines[3].Should().Be("255 204 153 255 204 153 255 204 153 255 204 153 255 204 153 255 204");
            lines[4].Should().Be("153 255 204 153 255 204 153 255 204 153 255 204 153");
            lines[5].Should().Be("255 204 153 255 204 153 255 204 153 255 204 153 255 204 153 255 204");
            lines[6].Should().Be("153 255 204 153 255 204 153 255 204 153 255 204 153");
        }

        [Fact]
        public void EndsInNewLineChar()
        {
            var c = new Canvas(10, 2);
            c.SetAllPixels(new Color(1f, 0.8f, 0.6f));
            var ppm = PPM.CanvasToPPM(c);
            ppm.Should().EndWith(Environment.NewLine);
        }

        [Fact(Skip = "creates file in My Pictures folder")]
        public void ProjectileTest()
        {
            var start = new Point(0, 1, 0);
            var vel = new Vector(1f, 1.8f, 0f).Normalize() * 11.25f;
            var p = new Projectile(start, vel);
            var w = new SimWorld(new Vector(0f, -0.1f, 0f), new Vector(-0.01f, 0, 0));
            var c = new Canvas(900, 550);
            var (x, y) = p.ToXY();
            y = 550 - y;
            var red = new Color(1f, 0f, 0f);
            while (c.IsInBounds(x, y))
            {
                c.WritePixel(red, x, y);
                p = p.Tick(w);
                (x, y) = p.ToXY();
                y = 550 - y;
            }

            PPM.ToFile(c, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "projectile");
        }

        [Fact]
        public void MustBePPMTypeThreeToImport()
        {
            var ppm = @"P32
1 1
255
0 0 0";
            var action = new Action(() => PPM.Parse(ppm));
            action.Should().Throw<FileLoadException>();
        }

        [Fact]
        public void CreatesCanvasWithProperDimensions()
        {
            var ppm = @"P3
10 2
255
0 0 0  0 0 0  0 0 0  0 0 0  0 0 0
0 0 0  0 0 0  0 0 0  0 0 0  0 0 0
0 0 0  0 0 0  0 0 0  0 0 0  0 0 0
0 0 0  0 0 0  0 0 0  0 0 0  0 0 0";
            var canvas = PPM.Parse(ppm);
            canvas.Width.Should().Be(10);
            canvas.Height.Should().Be(2);
        }

        [Fact]
        public void ProperlyScalesPixelColor()
        {
            var ppm = @"P3
4 3
255
255 127 0  0 127 255  127 255 0  255 255 255
0 0 0  255 0 0  0 255 0  0 0 255
255 255 0  0 255 255  255 0 255  127 127 127";
            var canvas = PPM.Parse(ppm);
            canvas.PixelAt(0, 0).Should().Be(new Color(1, 0.498f, 0));
            canvas.PixelAt(3, 0).Should().Be(new Color(1, 1, 1));
            canvas.PixelAt(2, 1).Should().Be(new Color(0, 1, 0));
            canvas.PixelAt(0, 2).Should().Be(new Color(1, 1, 0));
            canvas.PixelAt(3, 2).Should().Be(new Color(0.498f, 0.498f, 0.498f));
        }

        [Fact]
        public void HandlesComments()
        {
            var ppm = @"P3
# this is a comment
2 1
# this, too
255
# another comment
255 255 255
# oh, no, comments in the pixel data!
255 0 255";
            var canvas = PPM.Parse(ppm);
            canvas.PixelAt(0, 0).Should().Be(new Color(1, 1, 1));
            canvas.PixelAt(1, 0).Should().Be(new Color(1, 0, 1));
        }

        [Fact]
        public void HandlesColorDataOnDifferentLines()
        {
            var ppm = @"P3
1 1
255
51
153

204";
            var canvas = PPM.Parse(ppm);
            canvas.PixelAt(0, 0).Should().Be(new Color(0.2f, 0.6f, 0.8f));
        }
    }
}