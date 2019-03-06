using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class WorldTests
    {
        [Fact]
        public void DefaultWorld()
        {
            var world = new World();
            world.Objects.Should().HaveCount(2);
            world.Lights.Should().HaveCount(1);
        }

        [Fact]
        public void IntersectWorldWithRay()
        {
            var w = new World();
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = w.Intersect(r);
            xs.Should().HaveCount(4);
            xs[0].T.Should().Be(4f);
            xs[1].T.Should().Be(4.5f);
            xs[2].T.Should().Be(5.5f);
            xs[3].T.Should().Be(6f);
        }
    }
}