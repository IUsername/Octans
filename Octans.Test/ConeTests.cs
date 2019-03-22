using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ConeTests
    {
        private static (float t1, float t2) ExtractT1T2(IShape c, Point origin, Vector direction)
        {
            var n = direction.Normalize();
            var r = new Ray(origin, n);
            var xs = c.LocalIntersects(r);
            return (t1: xs[0].T, t2: xs[1].T);
        }

        private static int IntersectCount(IShape c, Point origin, Vector direction)
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
        public void IntersectionsWhenRayHitsCone()
        {
            var c = new Cone();
            ExtractT1T2(c, new Point(0, 0, -5), new Vector(0, 0, 1)).Should().Be((5f, 5f));
            var (t1, t2) = ExtractT1T2(c, new Point(1f, 1, -5), new Vector(-0.5f, -1f, 1f));
            t1.Should().BeApproximately(4.55006f, 0.0001f);
            t2.Should().BeApproximately(49.44994f, 0.0001f);
            (t1, t2) = ExtractT1T2(c, new Point(0, 0, -5f), new Vector(1, 1, 1));
            t1.Should().BeApproximately(8.66025f, 0.0001f);
            t2.Should().BeApproximately(8.66025f, 0.0001f);
        }

        [Fact]
        public void IntersectionsWhenRayParallelToOneHalf()
        {
            var c = new Cone();
            var n = new Vector(0, 1, 1).Normalize();
            var r = new Ray(new Point(0, 0, -1), n);
            var xs = c.LocalIntersects(r);
            xs.Count.Should().Be(1);
            xs[0].T.Should().BeApproximately(0.35355f, 0.0001f);
        }

        [Fact]
        public void ConeNormals()
        {
            var c = new Cone();
            c.LocalNormalAt(new Point(0, 0, 0), new Intersection(1, c)).Should().Be(new Vector(0, 0, 0));
            c.LocalNormalAt(new Point(1, 1, 1), new Intersection(1, c)).Should().Be(new Vector(1, -MathF.Sqrt(2f), 1));
            c.LocalNormalAt(new Point(-1, -1, 0), new Intersection(1, c)).Should().Be(new Vector(-1, 1, 0));
        }

        [Fact]
        public void InfiniteMinAndMaxByDefault()
        {
            var c = new Cone();
            c.Minimum.Should().Be(float.NegativeInfinity);
            c.Maximum.Should().Be(float.PositiveInfinity);
        }

        [Fact]
        public void IntersectingTruncatedCone()
        {
            var c = new Cone {Minimum = 1f, Maximum = 2f};
            IntersectCount(c, new Point(0, 1.5f, 0), new Vector(0.1f, 1, 0)).Should().Be(0);
            IntersectCount(c, new Point(0, 3, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 2, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 1, -5), new Vector(0, 0, 1)).Should().Be(0);
            IntersectCount(c, new Point(0, 1.5f, -2), new Vector(0, 0, 1)).Should().Be(2);
        }

        [Fact]
        public void DefaultConeIsNotClosed()
        {
            var c = new Cone();
            c.IsClosed.Should().BeFalse();
        }

        [Fact]
        public void IntersectingClosedCone()
        {
            var c = new Cone {Minimum = -0.5f, Maximum = 0.5f, IsClosed = true};
            IntersectCount(c, new Point(0, 0, -5), new Vector(0f, 1, 0)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, -0.25f), new Vector(0, 1, 1)).Should().Be(2);
            IntersectCount(c, new Point(0, 0, -0.25f), new Vector(0, 1, 0)).Should().Be(4);
        }

        [Fact]
        public void ConeNormalsAtCaps()
        {
            var c = new Cone {Minimum = 1f, Maximum = 2f, IsClosed = true};
            c.LocalNormalAt(new Point(0, 1, 0), new Intersection(1, c)).Should().Be(new Vector(0, -1, 0));
            c.LocalNormalAt(new Point(0.5f, 1, 0), new Intersection(1, c)).Should().Be(new Vector(0, -1, 0));
            c.LocalNormalAt(new Point(0, 1, 0.5f), new Intersection(1, c)).Should().Be(new Vector(0, -1, 0));
            c.LocalNormalAt(new Point(0, 2, 0), new Intersection(1, c)).Should().Be(new Vector(0, 1, 0));
            c.LocalNormalAt(new Point(0.5f, 2, 0), new Intersection(1, c)).Should().Be(new Vector(0, 1, 0));
            c.LocalNormalAt(new Point(0, 2, 0.5f), new Intersection(1, c)).Should().Be(new Vector(0, 1, 0));
        }

        [Fact]
        public void BoundsOfBoundCone()
        {
            var c = new Cone {Minimum = 1f, Maximum = 2f, IsClosed = true};
            var b = c.LocalBounds();
            b.Min.Should().Be(new Point(1, 1, 1));
            b.Max.Should().Be(new Point(2, 2, 2));
        }

        [Fact]
        public void BoundsOfUnboundCone()
        {
            var c = new Cone();
            var b = c.LocalBounds();
            b.Min.Should().Be(new Point(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity));
            b.Max.Should().Be(new Point(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
        }
    }
}