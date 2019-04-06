using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class CylinderTests
    {
        private static (float t1, float t2) ExtractT1T2(IGeometry c, Point origin, Vector direction)
        {
            var n = direction.Normalize();
            var r = new Ray(origin, n);
            var xs = c.LocalIntersects(r);
            return (t1: xs[0].T, t2: xs[1].T);
        }

        private static int IntersectCount(IGeometry c, Point origin, Vector direction)
        {
            var n = direction.Normalize();
            var r = new Ray(origin, n);
            var xs = c.LocalIntersects(r);
            return xs.Count;
        }

        [Fact]
        public void NoIntersectsWhenRayMissesCylinder()
        {
            var c = new Cylinder();
            IntersectCount(c, new Point(1, 0, 0), new Vector(0, 1, 0)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, 0), new Vector(0, 1, 0)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, -5), new Vector(1, 1, 1)).Should().Be(0);
        }

        [Fact]
        public void IntersectionsWhenRayHitsCylinder()
        {
            var c = new Cylinder();
            ExtractT1T2(c, new Point(1, 0, -5), new Vector(0, 0, 1)).Should().Be((5f, 5f));
            ExtractT1T2(c, new Point(0, 0, -5), new Vector(0, 0, 1)).Should().Be((4f, 6f));
            var (t1, t2) = ExtractT1T2(c, new Point(0.5f, 0, -5), new Vector(0.1f, 1f, 1f));
            t1.Should().BeApproximately(6.80798f, 0.0001f);
            t2.Should().BeApproximately(7.08872f, 0.0001f);
        }

        [Fact]
        public void CylinderNormals()
        {
            var c = new Cylinder();
            c.LocalNormalAt(new Point(1, 0, 0), new Intersection(1, c)).Should().Be(new Normal(1, 0, 0));
            c.LocalNormalAt(new Point(0, 5, -1), new Intersection(1, c)).Should().Be(new Normal(0, 0, -1));
            c.LocalNormalAt(new Point(0, -2, 1), new Intersection(1, c)).Should().Be(new Normal(0, 0, 1));
            c.LocalNormalAt(new Point(-1, 1, 0), new Intersection(1, c)).Should().Be(new Normal(-1, 0, 0));
        }

        [Fact]
        public void InfiniteMinAndMaxByDefault()
        {
            var c = new Cylinder();
            c.Minimum.Should().Be(float.NegativeInfinity);
            c.Maximum.Should().Be(float.PositiveInfinity);
        }

        [Fact]
        public void IntersectingTruncatedCylinder()
        {
            var c = new Cylinder {Minimum = 1f, Maximum = 2f};
            IntersectCount(c, new Point(0, 1.5f, 0), new Vector(0.1f, 1, 0)).Should().Be(0);
            IntersectCount(c, new Point(0, 3, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 2, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 1, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 1.5f, -2), new Vector(0, 0, 1)).Should().Be(2);
        }

        [Fact]
        public void DefaultCylinderIsNotClosed()
        {
            var c = new Cylinder();
            c.IsClosed.Should().BeFalse();
        }

        [Fact]
        public void IntersectingClosedCylinder()
        {
            var c = new Cylinder {Minimum = 1f, Maximum = 2f, IsClosed = true};
            IntersectCount(c, new Point(0, 3, 0), new Vector(0f, -1, 0)).Should().Be(2);
            IntersectCount(c, new Point(0, 3, -2), new Vector(0, -1, 2)).Should().Be(2);
            IntersectCount(c, new Point(0, 4, -2), new Vector(0, -1, 1)).Should().Be(2);
            IntersectCount(c, new Point(0, 0, -2), new Vector(0, 1, 2)).Should().Be(2);
            IntersectCount(c, new Point(0, -1, -2), new Vector(0, 1, 1)).Should().Be(2);
        }

        [Fact]
        public void CylinderNormalsAtCaps()
        {
            var c = new Cylinder {Minimum = 1f, Maximum = 2f, IsClosed = true};
            c.LocalNormalAt(new Point(0, 1, 0), new Intersection(1, c)).Should().Be(new Normal(0, -1, 0));
            c.LocalNormalAt(new Point(0.5f, 1, 0), new Intersection(1, c)).Should().Be(new Normal(0, -1, 0));
            c.LocalNormalAt(new Point(0, 1, 0.5f), new Intersection(1, c)).Should().Be(new Normal(0, -1, 0));
            c.LocalNormalAt(new Point(0, 2, 0), new Intersection(1, c)).Should().Be(new Normal(0, 1, 0));
            c.LocalNormalAt(new Point(0.5f, 2, 0), new Intersection(1, c)).Should().Be(new Normal(0, 1, 0));
            c.LocalNormalAt(new Point(0, 2, 0.5f), new Intersection(1, c)).Should().Be(new Normal(0, 1, 0));
        }

        [Fact]
        public void LocalBoundsIsInfiniteInYIfUnbound()
        {
            var c = new Cylinder();
            var b = c.LocalBounds();
            b.Min.Should().Be(new Point(-1, float.NegativeInfinity, -1));
            b.Max.Should().Be(new Point(1, float.PositiveInfinity, 1));
        }

        [Fact]
        public void LocalBoundsIsMinMaxInYIfBound()
        {
            var c = new Cylinder {Minimum = -2f, Maximum = 4f};
            var b = c.LocalBounds();
            b.Min.Should().Be(new Point(-1, -2, -1));
            b.Max.Should().Be(new Point(1, 4, 1));
        }
    }
}