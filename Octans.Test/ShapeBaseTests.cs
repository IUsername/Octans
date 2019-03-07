using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ShapeBaseTests
    {
        [Fact]
        public void IsShape()
        {
            var s = new TestShape();
            s.Should().BeAssignableTo<IShape>();
        }
       
        [Fact]
        public void DefaultTransformIsIdentity()
        {
            var s = new TestShape();
            s.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void CanChangeTransform()
        {
            var s = new TestShape();
            var t = Transforms.Translate(2, 3, 4);
            s.SetTransform(t);
            s.Transform.Should().Be(t);
        }

        [Fact]
        public void IntersectingScaledShapeWithRay()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var s = new TestShape();
            var t = Transforms.Scale(2, 2, 2);
            s.SetTransform(t);
            // ReSharper disable once UnusedVariable
            var xs = s.Intersects(r);
            s.SavedRay.Origin.Should().Be(new Point(0, 0, -2.5f));
            s.SavedRay.Direction.Should().Be(new Vector(0, 0, 0.5f));
        }

        [Fact]
        public void IntersectingTranslatedShapeWithRay()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var s = new TestShape();
            var t = Transforms.Translate(5, 0, 0);
            s.SetTransform(t);
            // ReSharper disable once UnusedVariable
            var xs = s.Intersects(r);
            s.SavedRay.Origin.Should().Be(new Point(-5, 0, -5f));
            s.SavedRay.Direction.Should().Be(new Vector(0, 0, 1f));
        }

        [Fact]
        public void NormalsAreNormalized()
        {
            var s = new TestShape();
            var n = s.NormalAt(new Point(MathF.Sqrt(3f) / 3f, MathF.Sqrt(3f) / 3f, MathF.Sqrt(3f) / 3f));
            (n == n.Normalize()).Should().BeTrue();
        }

        [Fact]
        public void NormalOnTranslatedShape()
        {
            var s = new TestShape();
            s.SetTransform(Transforms.Translate(0, 1, 0));
            var n = s.NormalAt(new Point(0, 1.70711f, -0.70711f));
            n.Should().Be(new Vector(0, 0.70711f, -0.70711f));
        }

        [Fact]
        public void NormalOnTransformedShape()
        {
            var s = new TestShape();
            s.SetTransform(Transforms.Scale(1f, 0.5f, 1f) * Transforms.RotateZ(MathF.PI / 5f));
            var n = s.NormalAt(new Point(0, MathF.Sqrt(2f) / 2f, -MathF.Sqrt(2f) / 2f));
            n.Should().Be(new Vector(0, 0.97014f, -0.24254f));
        }

        [Fact]
        public void HasDefaultMaterial()
        {
            var s = new Sphere();
            s.Material.Should().BeEquivalentTo(new Material());
        }

        [Fact]
        public void CanBeAssignedMaterial()
        {
            var s = new Sphere();
            var m = new Material {Ambient = 1f};
            s.SetMaterial(m);
            s.Material.Should().Be(m);
        }

        private class TestShape : ShapeBase
        {
            public Ray SavedRay { get; private set; }

            public override IReadOnlyList<Intersection> LocalIntersects(in Ray localRay)
            {
                SavedRay = localRay;
                return Intersections.Empty;
            }

            public override Vector LocalNormalAt(in Point localPoint)
            {
                // For testing locality of point.
                return new Vector(localPoint.X, localPoint.Y, localPoint.Z);
            }
        }
    }
}