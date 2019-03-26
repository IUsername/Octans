using FluentAssertions;
using Octans.Light;
using Xunit;

namespace Octans.Test.Light
{
    public class AreaLightTests
    {
        [Fact]
        public void CreateLight()
        {
            var corner = new Point(0, 0, 0);
            var v1 = new Vector(2, 0, 0);
            var v2 = new Vector(0, 0, 1);
            var light = new AreaLight(corner, v1, 4, v2, 2, Colors.White);
            light.Corner.Should().Be(corner);
            light.U.Should().Be(new Vector(0.5f, 0, 0));
            light.USteps.Should().Be(4);
            light.V.Should().Be(new Vector(0, 0, 0.5f));
            light.VSteps.Should().Be(2);
            light.Samples.Should().Be(8);
            light.Position.Should().Be(new Point(1, 0, 0.5f));
        }

        [Fact]
        public void DeterminePointOnLight()
        {
            var corner = new Point(0, 0, 0);
            var v1 = new Vector(2, 0, 0);
            var v2 = new Vector(0, 0, 1);
            var light = new AreaLight(corner, v1, 4, v2, 2, Colors.White);
            light.UVPoint(0, 0).Should().Be(new Point(0.25f, 0, 0.25f));
            light.UVPoint(1, 0).Should().Be(new Point(0.75f, 0, 0.25f));
            light.UVPoint(0, 1).Should().Be(new Point(0.25f, 0, 0.75f));
            light.UVPoint(2, 0).Should().Be(new Point(1.25f, 0, 0.25f));
            light.UVPoint(3, 1).Should().Be(new Point(1.75f, 0, 0.75f));
        }
    }
}