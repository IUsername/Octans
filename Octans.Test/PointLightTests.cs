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
            var position = Point.Zero;
            var light = new PointLight(position, intensity);
            light.Position.Should().Be(position);
            light.Intensity.Should().Be(intensity);
        }
    }
}