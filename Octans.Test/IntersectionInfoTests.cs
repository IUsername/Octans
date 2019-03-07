using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class IntersectionInfoTests
    {
        [Fact]
        public void ContainsPrecomputedData()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(4, shape);
            var comps = new IntersectionInfo(i, r);
            comps.T.Should().Be(4f);
            comps.Shape.Should().Be(shape);
            comps.Point.Should().Be(new Point(0, 0, -1));
            comps.Eye.Should().Be(new Vector(0, 0, -1));
            comps.Normal.Should().Be(new Vector(0, 0, -1));
        }

        [Fact]
        public void IsNotInsideWhenIntersectsOutsideSurface()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(4, shape);
            var comps = new IntersectionInfo(i, r);
            comps.IsInside.Should().BeFalse();
        }

        [Fact]
        public void IsInsideWhenIntersectsInsideSurface()
        {
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(1, shape);
            var comps = new IntersectionInfo(i, r);
            comps.IsInside.Should().BeTrue();
            comps.Normal.Should().Be(new Vector(0, 0, -1));
        }

        [Fact]
        public void ContainsPointOffset()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            shape.SetTransform(Transforms.Translate(0, 0, 1));
            var i = new Intersection(5, shape);
            var comps = new IntersectionInfo(i, r);
            comps.OverPoint.Z.Should().BeLessThan( -IntersectionInfo.Epsilon / 2f);
            comps.Point.Z.Should().BeGreaterThan(comps.OverPoint.Z);
        }
    }
}