using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class RayTests
    {
        [Fact]
        public void ContainsOriginAndDirection()
        {
            var p = Point.Create(1, 2, 3);
            var dir = Vector.Create(4, 5, 6);
            var ray = new Ray(p, dir);
            (ray.Origin == p).Should().BeTrue();
            (ray.Direction == dir).Should().BeTrue();
        }

        [Fact]
        public void ComputeDistance()
        {
            var r = new Ray(Point.Create(2,3,4), Vector.Create(1,0,0));
            r.Position(0f).Should().BeEquivalentTo(Point.Create(2, 3, 4));
            r.Position(0f).Should().BeEquivalentTo(Point.Create(2, 3, 4));
            r.Position(1f).Should().BeEquivalentTo(Point.Create(3, 3, 4));
            r.Position(-1f).Should().BeEquivalentTo(Point.Create(1, 3, 4));
            r.Position(2.5f).Should().BeEquivalentTo(Point.Create(4.5f, 3, 4));
        }
    }
}