using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PointLightTests
    {
        [Fact]
        public void ContainsPositionAndIntensity()
        {
            var intensity = new Color(1f, 1f, 1f);
            var position = Point.Create(0, 0, 0);
            var light = new PointLight(position, intensity);
            light.Position.Should().Be(position);
            light.Intensity.Should().Be(intensity);
        }
    }

    public class MaterialTests
    {
        [Fact]
        public void ContainsColorAmbientDiffuseSpecularAndShininess()
        {
            var m = new Material();
            m.Color.Should().Be(new Color(1f, 1f, 1f));
            m.Ambient.Should().Be(0.1f);
            m.Diffuse.Should().Be(0.9f);
            m.Specular.Should().Be(0.9f);
            m.Shininess.Should().Be(200f);
        }
    }
}