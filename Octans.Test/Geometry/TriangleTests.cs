using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class TriangleTests
    {
        [Fact]
        public void RequiresThreePoints()
        {
            var p1 = new Point(0, 1, 0);
            var p2 = new Point(-1, 0, 0);
            var p3 = new Point(1, 0, 0);
            var t = new Triangle(p1, p2, p3);
            t.P1.Should().Be(p1);
            t.P2.Should().Be(p2);
            t.P3.Should().Be(p3);
            t.E1.Should().Be(new Vector(-1, -1, 0));
            t.E2.Should().Be(new Vector(1, -1, 0));
            t.Normal.Should().Be(new Normal(0, 0, -1));
        }

        [Fact]
        public void CanFindNormalAtLocalPoint()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var n1 = t.LocalNormalAt(new Point(0, 0.5f, 0), new Intersection(1, t));
            var n2 = t.LocalNormalAt(new Point(-0.5f, 0.75f, 0), new Intersection(1, t));
            var n3 = t.LocalNormalAt(new Point(0.5f, 0.25f, 0), new Intersection(1, t));
            n1.Should().Be(t.Normal);
            n2.Should().Be(t.Normal);
            n3.Should().Be(t.Normal);
        }

        [Fact]
        public void NoIntersectionForParallelRay()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var r = new Ray(new Point(0, -1, -2), new Vector(0, 1, 0));
            var xs = t.LocalIntersects(r);
            xs.Count.Should().Be(0);
        }

        [Fact]
        public void RayMissesP1P3Edge()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var r = new Ray(new Point(1, 1, -2), new Vector(0, 0, 1));
            var xs = t.LocalIntersects(r);
            xs.Count.Should().Be(0);
        }

        [Fact]
        public void RayMissesP1P2Edge()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var r = new Ray(new Point(-1, 1, -2), new Vector(0, 0, 1));
            var xs = t.LocalIntersects(r);
            xs.Count.Should().Be(0);
        }

        [Fact]
        public void RayMissesP2P3Edge()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var r = new Ray(new Point(0, -1, -2), new Vector(0, 0, 1));
            var xs = t.LocalIntersects(r);
            xs.Count.Should().Be(0);
        }

        [Fact]
        public void RayStrikeReturnsT()
        {
            var t = new Triangle(new Point(0, 1, 0), new Point(-1, 0, 0), new Point(1, 0, 0));
            var r = new Ray(new Point(0, 0.5f, -2), new Vector(0, 0, 1));
            var xs = t.LocalIntersects(r);
            xs.Count.Should().Be(1);
            xs[0].T.Should().BeApproximately(2.0f, 0.0001f);
        }
    }
}