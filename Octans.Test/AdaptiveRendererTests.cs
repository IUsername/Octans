using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class AdaptiveRendererTests
    {
        [Fact]
        public void SamplesMorePointsUntilPassLimitReached()
        {
            var tester = new TestRenderer();
            tester.AddSample(new SubPixel(0,0,1,0,0), Colors.White);
            var delta = 0.1f;
            var aaa = new AdaptiveRenderer(3, 0.001f, tester);
            var c = aaa.Render(SubPixel.ForPixelCenter(0,0));
            c.Red.Should().BeApproximately(0.0f, delta);
            c.Blue.Should().BeApproximately(0.0f, delta);
            c.Green.Should().BeApproximately(0.0f, delta);
            tester.ProbeCount().Should().Be(14);
        }

        [Fact]
        public void SamplesMorePointsUntilToleranceReached()
        {
            var tester = new TestRenderer();
            tester.AddSample(new SubPixel(0, 0, 2, 1, 1), new Color(0.01f,0.01f,0.01f));
            tester.AddSample(new SubPixel(0, 0, 2, 0, 1), new Color(0.01f,0.01f,0.01f));
            tester.AddSample(new SubPixel(0, 0, 2, 1, 0), new Color(0.01f,0.01f,0.01f));
            tester.AddSample(new SubPixel(0, 0, 1, 0, 0), new Color(0.02f,0.02f,0.02f));
            var delta = 0.1f;
            var aaa = new AdaptiveRenderer(10, 0.01f, tester);
            var c = aaa.Render(SubPixel.ForPixelCenter(0, 0));
            c.Red.Should().BeApproximately(0.0f, delta);
            c.Blue.Should().BeApproximately(0.0f, delta);
            c.Green.Should().BeApproximately(0.0f, delta);
            tester.ProbeCount().Should().Be(7);
        }

        [Fact]
        public void RenderToCanvasWithAdaptiveAntiAliasing()
        {
            var from = new Point(0, 0, -5);
            var to = Point.Zero;
            var up = new Vector(0, 1, 0);
            var transform = Transforms.View(from, to, up);
            var w = World.Default();
            var width = 11;
            var height = 11;
            var c = new PinholeCamera(transform, MathF.PI / 2f, width, height);
            var s = new Scene(c, new RaytracedWorld(1, w));
            var aaa = new AdaptiveRenderer(2, 0.03f, s);
            var canvas = new Canvas(width, height);
            RenderContext.Render(canvas, aaa);
            var p = canvas.PixelAt(5, 5);
            p.Should().NotBe(new Color(0.38066f, 0.47583f, 0.2855f));
            p.Red.Should().BeApproximately(0.3429f, 0.005f);
            p.Green.Should().BeApproximately(0.4287f, 0.005f);
            p.Blue.Should().BeApproximately(0.2572f, 0.005f);
        }

        private class TestRenderer : IPixelRenderer
        {
            private readonly Dictionary<SubPixel, Color> _samples = new Dictionary<SubPixel, Color>();
            private Color _default = Colors.Black;
            private int _probed = 0;

            public void AddSample(SubPixel subPixel, Color color)
            {
                _samples.Add(subPixel, color);
            }

            public void SetDefaultColor(Color color)
            {
                _default = color;
            }

            public int ProbeCount()
            {
                return _probed;
            }

            public Color Render(in SubPixel sp)
            {
                _probed++;
                return _samples.TryGetValue(sp, out var color) ? color : _default;
            }
        }
    }
}