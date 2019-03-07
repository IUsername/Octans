using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class IntersectionTests
    {
        [Fact]
        public void ContainsDistanceAndObject()
        {
            var s = new Sphere();
            var i = new Intersection(3.5f, s);
            i.T.Should().Be(3.5f);
            i.Shape.Should().Be(s);
        }

        [Fact]
        public void HitOnSmallestPositiveIntersection()
        {
            var s = new Sphere();
            var i1 = new Intersection(1f, s);
            var i2 = new Intersection(2f, s);
            var xs = new Intersections(i1, i2);
            var hit = xs.Hit();
            hit?.Should().Be(i1);
        }

        [Fact]
        public void HitOnlyPositiveIntersection()
        {
            var s = new Sphere();
            var i1 = new Intersection(-1f, s);
            var i2 = new Intersection(1f, s);
            var xs = new Intersections(i1, i2);
            var hit = xs.Hit();
            hit?.Should().Be(i2);
        }

        [Fact]
        public void HitNoneOnAllNegativeIntersections()
        {
            var s = new Sphere();
            var i1 = new Intersection(-2f, s);
            var i2 = new Intersection(-1f, s);
            var xs = new Intersections(i1, i2);
            var hit = xs.Hit();
            hit.HasValue.Should().BeFalse();
        }

        [Fact]
        public void SortsIntersectionsByT()
        {
            var s = new Sphere();
            var i1 = new Intersection(5f, s);
            var i2 = new Intersection(7f, s);
            var i3 = new Intersection(-3f, s);
            var i4 = new Intersection(2f, s);
            var xs = new Intersections(i1, i2, i3, i4);
            var hit = xs.Hit();
            hit?.Should().Be(i4);
        }
    }
}