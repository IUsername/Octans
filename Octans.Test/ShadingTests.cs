using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ShadingTests
    {
        [Fact]
        public void EyeBetweenLightAndSurface()
        {
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, light, position, eyeV, normalV);
            result.Should().BeEquivalentTo(Color.RGB(1.9f, 1.9f, 1.9f));
        }

        [Fact]
        public void EyeOffset45Deg()
        {
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, MathF.Sqrt(2) / 2, -MathF.Sqrt(2) / 2);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, light, position, eyeV, normalV);
            result.Should().BeEquivalentTo(Color.RGB(1.0f, 1.0f, 1.0f));
        }

        [Fact]
        public void LightOffset45Deg()
        {
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1f);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 10, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, light, position, eyeV, normalV);
            result.Should().BeEquivalentTo(Color.RGB(0.7364f, 0.7364f, 0.7364f));
        }

        [Fact]
        public void FullReflectionOfLightOffset45Deg()
        {
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, -MathF.Sqrt(2) / 2, -MathF.Sqrt(2) / 2);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 10, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, light, position, eyeV, normalV);
            result.Should().BeEquivalentTo(Color.RGB(1.6364f, 1.6364f, 1.6364f));
        }

        [Fact]
        public void LightBehindSurface()
        {
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1f);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, 10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, light, position, eyeV, normalV);
            result.Should().BeEquivalentTo(Color.RGB(0.1f, 0.1f, 0.1f));
        }
    }
}