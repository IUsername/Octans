using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class RayTests
    {
        [Fact]
        public void ContainsOriginAndDirection()
        {
            var p = new Point(1, 2, 3);
            var dir = new Vector(4, 5, 6);
            var ray = new Ray(p, dir);
            (ray.Origin == p).Should().BeTrue();
            (ray.Direction == dir).Should().BeTrue();
        }

        [Fact]
        public void ComputeDistance()
        {
            var r = new Ray(new Point(2, 3, 4), new Vector(1, 0, 0));
            r.Position(0f).Should().BeEquivalentTo(new Point(2, 3, 4));
            r.Position(0f).Should().BeEquivalentTo(new Point(2, 3, 4));
            r.Position(1f).Should().BeEquivalentTo(new Point(3, 3, 4));
            r.Position(-1f).Should().BeEquivalentTo(new Point(1, 3, 4));
            r.Position(2.5f).Should().BeEquivalentTo(new Point(4.5f, 3, 4));
        }

        [Fact]
        public void Translate()
        {
            var p = new Point(1, 2, 3);
            var dir = new Vector(0, 1, 0);
            var r1 = new Ray(p, dir);
            var t = Transform.Translate(3, 4, 5);
            var r2 = t * r1;
            r2.Origin.Should().BeEquivalentTo(new Point(4, 6, 8));
            r2.Direction.Should().BeEquivalentTo(new Vector(0, 1, 0));
        }

        [Fact]
        public void Scale()
        {
            var p = new Point(1, 2, 3);
            var dir = new Vector(0, 1, 0);
            var r1 = new Ray(p, dir);
            var t = Transform.Scale(2, 3, 4);
            var r2 = t * r1;
            r2.Origin.Should().BeEquivalentTo(new Point(2, 6, 12));
            r2.Direction.Should().BeEquivalentTo(new Vector(0, 3, 0));
        }
    }
}