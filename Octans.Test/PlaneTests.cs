using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PlaneTests
    {
        [Fact]
        public void NormalOfPlaneIsConstantEverywhere()
        {
            var p = new Plane();
            var n1 = p.LocalNormalAt(new Point(0, 0, 0));
            var n2 = p.LocalNormalAt(new Point(10, 0, -10));
            var n3 = p.LocalNormalAt(new Point(-5, 0, 1500));
            n1.Should().Be(new Vector(0, 1, 0));
            n2.Should().Be(new Vector(0, 1, 0));
            n3.Should().Be(new Vector(0, 1, 0));
        }

        [Fact]
        public void IntersectsOfParallelRayToPlaneIsEmpty()
        {
            var p = new Plane();
            var r = new Ray(new Point(0, 10, 0), new Vector(0, 0, 1));
            var xs = p.LocalIntersects(r);
            xs.Should().BeEmpty();
        }

        [Fact]
        public void IntersectsOfCoplanarRayToPlaneIsEmpty()
        {
            var p = new Plane();
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var xs = p.LocalIntersects(r);
            xs.Should().BeEmpty();
        }

        [Fact]
        public void RayFromAboveIntersectPlane()
        {
            var p = new Plane();
            var r = new Ray(new Point(0, 1, 0), new Vector(0, -1, 0));
            var xs = p.LocalIntersects(r);
            xs.Should().HaveCount(1);
            xs[0].Shape.Should().Be(p);
        }

        [Fact]
        public void RayFromBelowIntersectPlane()
        {
            var p = new Plane();
            var r = new Ray(new Point(0, -1, 0), new Vector(0, 1, 0));
            var xs = p.LocalIntersects(r);
            xs.Should().HaveCount(1);
            xs[0].Shape.Should().Be(p);
        }
    }
}