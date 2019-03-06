using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SphereTests
    {
        [Fact]
        public void NonEdgeRayIntersectsSphereAtTwoPoints()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(4.0f);
            xs[1].T.Should().Be(6.0f);
            xs[0].Obj.Should().Be(s);
            xs[1].Obj.Should().Be(s);
        }

        [Fact]
        public void NonIntersectionReturnsZeroCount()
        {
            var r = new Ray(Point.Create(0, 2, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(0);
        }

        [Fact]
        public void RayOriginatingInsideSphereHasOneNegativeIntersect()
        {
            var r = new Ray(Point.Create(0, 0, 0), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-1.0f);
            xs[1].T.Should().Be(1.0f);
        }

        [Fact]
        public void SphereBehindRayHasTwoNegativeIntersects()
        {
            var r = new Ray(Point.Create(0, 0, 5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-6.0f);
            xs[1].T.Should().Be(-4.0f);
        }

        [Fact]
        public void DefaultTransformIsIdentity()
        {
            var s = new Sphere();
            s.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void CanChangeTransform()
        {
            var s = new Sphere();
            var t = Transforms.Translate(2, 3, 4);
            s.SetTransform(t);
            s.Transform.Should().Be(t);
        }

        [Fact]
        public void IntersectingScaledSphere()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var t = Transforms.Scale(2, 2, 2);
            s.SetTransform(t);
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(3f);
            xs[1].T.Should().Be(7f);
        }

        [Fact]
        public void IntersectingTranslatedSphere()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var t = Transforms.Translate(5, 0, 0);
            s.SetTransform(t);
            var xs = s.Intersect(r);
            xs.Should().HaveCount(0);
        }
    }
}