using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class CubeTests
    {
        private static (float t1, float t2) ExtractT1T2(IGeometry c, Point origin, Vector direction)
        {
            var r = new Ray(origin, direction);
            var xs = c.LocalIntersects(r);
            return (t1: xs[0].T, t2: xs[1].T);
        }

        private static int IntersectCount(IGeometry c, Point origin, Vector direction)
        {
            var r = new Ray(origin, direction);
            var xs = c.LocalIntersects(r);
            return xs.Count;
        }

        private static Normal NormalAtPoint(Cube cube, Point point) =>
            cube.LocalNormalAt(point, new Intersection(1, cube));

        [Fact]
        public void ProperlyIntersectsFaces()
        {
            var c = new Cube();

            // +X
            ExtractT1T2(c, new Point(5f, 0.5f, 0f), new Vector(-1, 0, 0))
                .Should()
                .Be((4f, 6f));

            // -X
            ExtractT1T2(c, new Point(-5f, 0.5f, 0f), new Vector(1, 0, 0))
                .Should()
                .Be((4f, 6f));

            // +Y
            ExtractT1T2(c, new Point(0.5f, 5f, 0f), new Vector(0, -1, 0))
                .Should()
                .Be((4f, 6f));

            // -Y
            ExtractT1T2(c, new Point(0.5f, -5f, 0f), new Vector(0, 1, 0))
                .Should()
                .Be((4f, 6f));

            // +Z
            ExtractT1T2(c, new Point(0.5f, 0f, 5f), new Vector(0, 0, -1))
                .Should()
                .Be((4f, 6f));

            // -Z
            ExtractT1T2(c, new Point(0.5f, 0f, -5f), new Vector(0, 0, 1))
                .Should()
                .Be((4f, 6f));

            // Inside
            ExtractT1T2(c, new Point(0f, 0.5f, 0f), new Vector(0, 0, 1))
                .Should()
                .Be((-1f, 1f));
        }

        [Fact]
        public void NoIntersectsWhenRayMissesCube()
        {
            var c = new Cube();
            IntersectCount(c, new Point(-2, 0, 0), new Vector(0.2673f, 0.5345f, 0.8018f)).Should().Be(0);
            IntersectCount(c, new Point(0, -2, 0), new Vector(0.8018f, 0.2673f, 0.5345f)).Should().Be(0);
            IntersectCount(c, new Point(0, 0, -2), new Vector(0.5345f, 0.8018f, 0.2673f)).Should().Be(0);
            IntersectCount(c, new Point(2, 0, 2), new Vector(0f, 0f, -1f)).Should().Be(0);
            IntersectCount(c, new Point(0, 2, 2), new Vector(0f, -1f, 0f)).Should().Be(0);
            IntersectCount(c, new Point(2, 2, 0), new Vector(-1f, 0f, 0f)).Should().Be(0);
        }

        [Fact]
        public void ReturnProperNormalVectors()
        {
            var c = new Cube();
            NormalAtPoint(c, new Point(1, 0.5f, -0.8f)).Should().Be(new Normal(1, 0, 0));
            NormalAtPoint(c, new Point(-1, -0.2f, 0.9f)).Should().Be(new Normal(-1, 0, 0));
            NormalAtPoint(c, new Point(-0.4f, 1f, -0.1f)).Should().Be(new Normal(0, 1, 0));
            NormalAtPoint(c, new Point(0.3f, -1f, -0.7f)).Should().Be(new Normal(0, -1, 0));
            NormalAtPoint(c, new Point(-0.6f, 0.3f, 1f)).Should().Be(new Normal(0, 0, 1));
            NormalAtPoint(c, new Point(0.4f, 0.4f, -1f)).Should().Be(new Normal(0, 0, -1));
            NormalAtPoint(c, new Point(1f, 1f, 1f)).Should().Be(new Normal(1, 0, 0));
            NormalAtPoint(c, new Point(-1f, -1f, -1f)).Should().Be(new Normal(-1, 0, 0));
        }

        [Fact]
        public void LocalBoundsIsUnitAABB()
        {
            var c = new Cube();
            var b = c.LocalBounds();
            b.Min.Should().Be(new Point(-1, -1, -1));
            b.Max.Should().Be(new Point(1, 1, 1));
        }
    }
}