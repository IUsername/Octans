using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class BoundsTests
    {
        [Fact]
        public void ContainsMinAndMaxPoints()
        {
            var pMin = new Point(-1, -2, -3);
            var pMax = new Point(3, 2, 1);
            var b = new Bounds(pMin, pMax);
            b.Min.Should().Be(pMin);
            b.Max.Should().Be(pMax);
        }

        [Fact]
        public void IsEmptyIfCreatedFromEmpty()
        {
            var pMin = new Point(-1, -2, -3);
            var pMax = new Point(3, 2, 1);
            var b1 = new Bounds(pMin, pMax);
            var b2 = new Bounds(Point.Zero, Point.Zero);
            var b3 = Bounds.Empty;
            b1.IsEmpty.Should().BeFalse();
            b2.IsEmpty.Should().BeFalse();
            b3.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void CanAddBounds()
        {
            var b1 = new Bounds(new Point(-1, -2, -3), new Point(3, 2, 1));
            var b2 = new Bounds(new Point(-3, -1, -3), new Point(2, 4, 1));
            var b = b1 + b2;
            b.Min.Should().Be(new Point(-3, -2, -3));
            b.Max.Should().Be(new Point(3, 4, 1));
            var b3 = new Bounds(new Point(float.NegativeInfinity, 0, float.NegativeInfinity), new Point(float.PositiveInfinity, 0, float.PositiveInfinity));
            b += b3;
            b.Min.Should().Be(new Point(float.NegativeInfinity, -2, float.NegativeInfinity));
            b.Max.Should().Be(new Point(float.PositiveInfinity, 4, float.PositiveInfinity));
        }

        [Fact]
        public void ReturnsTrueIfRayIntersectsBoundsFromOutside()
        {
            var pMin = new Point(0, 0, 0);
            var pMax = new Point(2, 3, 4);
            var b = new Bounds(pMin, pMax);
            var r = new Ray(new Point(1, 1, -10), new Vector(0, 0, 1));
            b.DoesIntersect(r).Should().BeTrue();
            r = new Ray(new Point(-1, 1, 2), new Vector(1, 0, 0));
            b.DoesIntersect(r).Should().BeTrue();
        }

        [Fact]
        public void ReturnsTrueIfRayIntersectsBoundsFromInside()
        {
            var pMin = new Point(-1, -1, -1);
            var pMax = new Point(1, 1, 1);
            var b = new Bounds(pMin, pMax);
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            b.DoesIntersect(r).Should().BeTrue();
        }

        [Fact]
        public void ReturnsFalseIfRayDoesNotIntersectsBounds()
        {
            var pMin = new Point(0, 0, 0);
            var pMax = new Point(2, 3, 4);
            var b = new Bounds(pMin, pMax);
            var r = new Ray(new Point(5, 0, -10), new Vector(0, 0, 1));
            b.DoesIntersect(r).Should().BeFalse();
            r = new Ray(new Point(1, 4, -10), new Vector(0, 0, 1));
            b.DoesIntersect(r).Should().BeFalse();
        }

        [Fact]
        public void ReturnsTrueIfRayIntersectsInfinitePlane()
        {
            var pMin = new Point(float.NegativeInfinity, 0, float.NegativeInfinity);
            var pMax = new Point(float.PositiveInfinity, 0, float.PositiveInfinity);
            var b = new Bounds(pMin, pMax);
            var r = new Ray(new Point(0, 10, 0), new Vector(0, 1, 0));
            b.DoesIntersect(r).Should().BeTrue();
        }
    }
}