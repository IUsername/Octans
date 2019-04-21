using System;
using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Geometry
{
    public class GroupTests
    {
        [Fact]
        public void HasIdentityMatrixByDefault()
        {
            var g = new Group();
            g.Transform.Should().Be(Transform.Identity);
        }

        [Fact]
        public void IsEmptyByDefault()
        {
            var g = new Group();
            g.Children.Should().BeEmpty();
        }

        [Fact]
        public void AddingChildToGroupAssignsGroupAsParent()
        {
            var g = new Group();
            var s = new TestGeometry();
            g.AddChild(s);
            s.Parent.Should().Be(g);
            g.Children.Should().OnlyContain(c => c == s);
        }

        [Fact]
        public void NotIntersectionsWithEmptyGroup()
        {
            var g = new Group();
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var xs = g.LocalIntersects(r);
            xs.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectionsWithNonEmptyGroup()
        {
            var g = new Group();
            var s1 = new Sphere();
            var s2 = new Sphere();
            var s3 = new Sphere();
            s2.SetTransform(Transform.TranslateZ(-3f));
            s3.SetTransform(Transform.TranslateX(5f));
            g.AddChild(s1);
            g.AddChild(s2);
            g.AddChild(s3);
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = g.LocalIntersects(r).ToSorted();
            xs.Should().HaveCount(4);
            xs[0].Geometry.Should().Be(s2);
            xs[1].Geometry.Should().Be(s2);
            xs[2].Geometry.Should().Be(s1);
            xs[3].Geometry.Should().Be(s1);
        }

        [Fact]
        public void CanTranslateGroup()
        {
            var g = new Group();
            g.SetTransform(Transform.Scale(2f));
            var s = new Sphere();
            s.SetTransform(Transform.TranslateX(5f));
            g.AddChild(s);
            var r = new Ray(new Point(10, 0, -10), new Vector(0, 0, 1));
            var xs = g.Intersects(r);
            xs.Count.Should().Be(2);
        }

        [Fact]
        public void LocalBoundsConsidersTransformOfChild()
        {
            var g = new Group();
            var c = new Cube();
            c.SetTransform(Transform.RotateZ(System.MathF.PI / 4));
            g.AddChild(c);
            var b = g.LocalBounds();
            b.Min.Should().Be(new Point(-1 / System.MathF.Sin(System.MathF.PI / 4), -1 / System.MathF.Sin(System.MathF.PI / 4), -1));
            b.Max.Should().Be(new Point(1 / System.MathF.Sin(System.MathF.PI / 4), 1 / System.MathF.Sin(System.MathF.PI / 4), 1));
        }

        [Fact]
        public void LocalBoundsConsidersAllChildren()
        {
            var s = new Sphere();
            s.SetTransform(Transform.Translate(2, 5, -3) * Transform.Scale(2f));
            var c = new Cylinder {Minimum = -2, Maximum = 2};
            c.SetTransform(Transform.Translate(-4, -1, 4) * Transform.Scale(0.5f, 1, 0.5f));
            var g = new Group();
            g.AddChild(s);
            g.AddChild(c);
            var b = g.LocalBounds();
            b.Min.Should().Be(new Point(-4.5f, -3, -5));
            b.Max.Should().Be(new Point(4f, 7, 4.5f));
        }

        [Fact]
        public void ReturnsEmptyIntersectionsIfRayMissesBoundingBox()
        {
            var g = new Group();
            var c = new Cube();
            c.SetTransform(Transform.TranslateX(10));
            g.AddChild(c);
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var intersects = g.Intersects(r);
            intersects.Count.Should().Be(0);
        }

        [Fact]
        public void IntersectDoesNotTestChildrenIfRayMissesBounds()
        {
            var c = new TestGeometry();
            var g = new Group();
            g.AddChild(c);
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 1, 0));
            var xs = g.Intersects(r);
            // Child not tested so SavedRay remains default.
            c.SavedRay.Should().Be(Ray.Undefined);
        }

        [Fact]
        public void IntersectTestChildrenIfRayHitsBounds()
        {
            var c = new TestGeometry();
            var g = new Group();
            g.AddChild(c);
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = g.Intersects(r);
            // Child tested so SavedRay in not default.
            c.SavedRay.Should().NotBe(Ray.Undefined);
        }

        [Fact]
        public void PartitionChildren()
        {
            var s1 = new Sphere();
            s1.SetTransform(Transform.Translate(-2, 0, 0));
            var s2 = new Sphere();
            s2.SetTransform(Transform.Translate(2, 0, 0));
            var s3 = new Sphere();
            var g = new Group(s1, s2, s3);
            var (left, right) = g.PartitionChildren();
            g.Children[0].Should().Be(s3);
            left.Should().Contain(new[] {s1});
            right.Should().Contain(new[] {s2});
        }

        [Fact]
        public void CreateSubGroup()
        {
            var s1 = new Sphere();
            var s2 = new Sphere();
            var g = new Group();
            g.AddSubgroup(new IGeometry[] {s1, s2});
            g.Children.Should().HaveCount(1);
            g.Children[0].Should().BeAssignableTo<Group>();
            var sg = (Group) g.Children[0];
            sg.Children.Should().Contain(new[] {s1, s2});
        }

        [Fact]
        public void DivideGroup()
        {
            var s1 = new Sphere();
            s1.SetTransform(Transform.Translate(-2, 0, 0));
            var s2 = new Sphere();
            s2.SetTransform(Transform.Translate(2, 0, 0));
            var s3 = new Sphere();
            var g = new Group(s1, s2, s3);
            g.Divide(1);
            g.Children[0].Should().Be(s3);
            var sg1 = (Group) g.Children[1];
            sg1.Children.Should().Contain(new[] {s1});
            var sg2 = (Group) g.Children[2];
            sg2.Children.Should().Contain(new[] {s2});
        }

        [Fact]
        public void DivideGroupWithHigherThreshold()
        {
            var s1 = new Sphere();
            s1.SetTransform(Transform.Translate(-2, 0, 0));
            var s2 = new Sphere();
            s2.SetTransform(Transform.Translate(2, 0, 0));
            var s3 = new Sphere();
            s3.SetTransform(Transform.Translate(2, -1, 0));
            var sg = new Group(s1, s2, s3);
            var s4 = new Sphere();
            var g = new Group(sg, s4);
            g.Divide(3);
            g.Children[0].Should().Be(sg);
            g.Children[1].Should().Be(s4);
            var dsg = (Group) g.Children[0];
            var dsg1 = (Group) dsg.Children[0];
            var dsg2 = (Group) dsg.Children[1];
            dsg1.Children.Should().Contain(new[] {s1});
            dsg2.Children.Should().Contain(new[] {s2, s3});
        }
    }
}