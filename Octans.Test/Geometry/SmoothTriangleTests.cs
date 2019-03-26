using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class SmoothTriangleTests
    {
        private static SmoothTriangle DefaultTriangle()
        {
            var p1 = new Point(0, 1, 0);
            var p2 = new Point(-1, 0, 0);
            var p3 = new Point(1, 0, 0);
            var n1 = new Vector(0, 1, 0);
            var n2 = new Vector(-1, 0, 0);
            var n3 = new Vector(1, 0, 0);
            var tri = new SmoothTriangle(p1, p2, p3, n1, n2, n3);
            return tri;
        }

        [Fact]
        public void RequiresPointsAndNormals()
        {
            var p1 = new Point(0, 1, 0);
            var p2 = new Point(-1, 0, 0);
            var p3 = new Point(1, 0, 0);
            var n1 = new Vector(0, 1, 0);
            var n2 = new Vector(-1, 0, 0);
            var n3 = new Vector(1, 0, 0);
            var tri = new SmoothTriangle(p1, p2, p3, n1, n2, n3);
            tri.P1.Should().Be(p1);
            tri.P2.Should().Be(p2);
            tri.P3.Should().Be(p3);
            tri.N1.Should().Be(n1);
            tri.N2.Should().Be(n2);
            tri.N3.Should().Be(n3);
        }

        [Fact]
        public void IntersectionStoresUV()
        {
            var tri = DefaultTriangle();
            var r = new Ray(new Point(-0.2f, 0.3f, -2), new Vector(0, 0, 1));
            var xs = tri.LocalIntersects(r);
            xs[0].U.Should().BeApproximately(0.45f, 0.0001f);
            xs[0].V.Should().BeApproximately(0.25f, 0.0001f);
        }

        [Fact]
        public void UsesUVToInterpolateNormal()
        {
            var tri = DefaultTriangle();
            var i = new Intersection(1, tri, 0.45f, 0.25f);
            var n = tri.NormalAt(new Point(0, 0, 0), i);
            n.Should().Be(new Vector(-0.5547f, 0.83205f, 0));
        }

        [Fact]
        public void InterpolatesNormalsForIntersections()
        {
            var tri = DefaultTriangle();
            var i = new Intersection(1, tri, 0.45f, 0.25f);
            var r = new Ray(new Point(-0.2f, 0.3f, -2), new Vector(0, 0, 1));
            var xs = Intersections.Create(i);
            var comps = new IntersectionInfo(i, r, xs);
            comps.Normal.Should().Be(new Vector(-0.5547f, 0.83205f, 0));
        }
    }
}