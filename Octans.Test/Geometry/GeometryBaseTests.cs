using System;
using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class GeometryBaseTests
    {
        [Fact]
        public void IsShape()
        {
            var s = new TestGeometry();
            s.Should().BeAssignableTo<IGeometry>();
        }

        [Fact]
        public void DefaultTransformIsIdentity()
        {
            var s = new TestGeometry();
            s.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void CanChangeTransform()
        {
            var s = new TestGeometry();
            var t = Transforms.Translate(2, 3, 4);
            s.SetTransform(t);
            s.Transform.Should().Be(t);
        }

        [Fact]
        public void IntersectingScaledShapeWithRay()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var s = new TestGeometry();
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
            var s = new TestGeometry();
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
            var s = new TestGeometry();
            var n = s.NormalAt(new Point(MathF.Sqrt(3f) / 3f, MathF.Sqrt(3f) / 3f, MathF.Sqrt(3f) / 3f),
                               new Intersection(1f, s));
            (n == n.Normalize()).Should().BeTrue();
        }

        [Fact]
        public void NormalOnTranslatedShape()
        {
            var s = new TestGeometry();
            s.SetTransform(Transforms.Translate(0, 1, 0));
            var n = s.NormalAt(new Point(0, 1.70711f, -0.70711f), new Intersection(1f, s));
            n.Should().Be(new Vector(0, 0.70711f, -0.70711f));
        }

        [Fact]
        public void NormalOnTransformedShape()
        {
            var s = new TestGeometry();
            s.SetTransform(Transforms.Scale(1f, 0.5f, 1f) * Transforms.RotateZ(MathF.PI / 5f));
            var n = s.NormalAt(new Point(0, MathF.Sqrt(2f) / 2f, -MathF.Sqrt(2f) / 2f), new Intersection(1f, s));
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

        [Fact]
        public void HasNoParentByDefault()
        {
            var s = new TestGeometry();
            s.Parent.Should().BeNull();
        }

        [Fact]
        public void ConvertsPointFromWorldToObjectSpace()
        {
            var g1 = new Group();
            g1.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var g2 = new Group();
            g2.SetTransform(Transforms.Scale(2));
            g1.AddChild(g2);
            var s = new Sphere();
            s.SetTransform(Transforms.TranslateX(5));
            g2.AddChild(s);
            var p = s.ToLocal(new Point(-2, 0, -10));
            p.Should().BeEquivalentTo(new Point(0, 0, -1));
        }

        [Fact]
        public void ConvertsNormalsFromObjectToWorldSpace()
        {
            var g1 = new Group();
            g1.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var g2 = new Group();
            g2.SetTransform(Transforms.Scale(1, 2, 3));
            g1.AddChild(g2);
            var s = new Sphere();
            s.SetTransform(Transforms.TranslateX(5));
            g2.AddChild(s);
            var n = s.NormalToWorld(new Vector(MathF.PI / 3, MathF.PI / 3, MathF.PI / 3));
            n.Should().Be(new Vector(0.2857f, 0.4286f, -0.8571f));
        }

        [Fact]
        public void FindNormalOnChild()
        {
            var g1 = new Group();
            g1.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var g2 = new Group();
            g2.SetTransform(Transforms.Scale(1, 2, 3));
            g1.AddChild(g2);
            var s = new Sphere();
            s.SetTransform(Transforms.TranslateX(5));
            g2.AddChild(s);
            var n = s.NormalAt(new Point(1.7321f, 1.1547f, -5.5774f), new Intersection(1f, s));
            n.Should().Be(new Vector(0.2857f, 0.4286f, -0.8571f));
        }
    }
}