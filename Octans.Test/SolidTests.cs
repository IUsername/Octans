using System.Linq;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SolidTests
    {
        [Fact]
        public void ContainsOperationAndTwoShapes()
        {
            var s1 = new Sphere();
            var s2 = new Cube();
            var c = new Solid(SolidOp.Union, s1,s2);
            c.Left.Should().Be(s1);
            c.Right.Should().Be(s2);
            s1.Parent.Should().Be(c);
            s2.Parent.Should().Be(c);
        }

        [Fact]
        public void OperationsRules()
        {
            // ReSharper disable ArgumentsStyleLiteral
            Solid.IntersectionAllowed(SolidOp.Union, lHit: true, inL: true, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: true, inL: true, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: true, inL: false, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: true, inL: false, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: false, inL: true, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: false, inL: true, inR: false).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: false, inL: false, inR: true).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Union, lHit: false, inL: false, inR: false).Should().BeTrue();

            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: true, inL: true, inR: true).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: true, inL: true, inR: false).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: true, inL: false, inR: true).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: true, inL: false, inR: false).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: false, inL: true, inR: true).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: false, inL: true, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: false, inL: false, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Intersection, lHit: false, inL: false, inR: false).Should().BeFalse();

            Solid.IntersectionAllowed(SolidOp.Difference, lHit: true, inL: true, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: true, inL: true, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: true, inL: false, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: true, inL: false, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: false, inL: true, inR: true).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: false, inL: true, inR: false).Should().BeTrue();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: false, inL: false, inR: true).Should().BeFalse();
            Solid.IntersectionAllowed(SolidOp.Difference, lHit: false, inL: false, inR: false).Should().BeFalse();
            // ReSharper restore ArgumentsStyleLiteral
        }

        [Fact]
        public void FiltersIntersections()
        {
            var s1 = new Sphere();
            var s2 = new Cube();
            var xs = Intersections.Create(new Intersection(1, s1), 
                                          new Intersection(2, s2), 
                                          new Intersection(3, s1),
                                          new Intersection(4, s2));
            var c = new Solid(SolidOp.Union, s1, s2);
            var result = c.FilterIntersections(xs);
            result.Should().HaveCount(2);
            result[0].Should().Be(xs[0]);
            result[1].Should().Be(xs[3]);

            c = new Solid(SolidOp.Intersection, s1, s2);
            result = c.FilterIntersections(xs);
            result.Should().HaveCount(2);
            result[0].Should().Be(xs[1]);
            result[1].Should().Be(xs[2]);

            c = new Solid(SolidOp.Difference, s1, s2);
            result = c.FilterIntersections(xs);
            result.Should().HaveCount(2);
            result[0].Should().Be(xs[0]);
            result[1].Should().Be(xs[1]);
        }

        [Fact]
        public void RayMissesSolid()
        {
            var c = new Solid(SolidOp.Union, new Sphere(), new Cube());
            var r = new Ray(new Point(0, 2, -5), new Vector(0, 0, 1));
            var xs = c.LocalIntersects(in r);
            xs.Should().BeEmpty();
        }

        [Fact]
        public void RayHitsSolid()
        {
            var s1 = new Sphere();
            var s2 = new Sphere();
            s2.SetTransform(Transforms.TranslateZ(0.5f));
            var c = new Solid(SolidOp.Union, s1,s2);
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = c.LocalIntersects(in r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(4f);
            xs[0].Shape.Should().Be(s1);
            xs[1].T.Should().Be(6.5f);
            xs[1].Shape.Should().Be(s2);
        }

        [Fact]
        public void BoundsContainsChildren()
        {
            var l = new Sphere();
            var r = new Sphere();
            r.SetTransform(Transforms.Translate(2,3,4));
            var s = new Solid(SolidOp.Difference, l,r);
            var b = s.LocalBounds();
            b.Min.Should().Be(new Point(-1, -1, -1));
            b.Max.Should().Be(new Point(3, 4, 5));
        }

        [Fact]
        public void IntersectDoesNotTestChildrenIfRayMissesBounds()
        {
            var l = new TestShape();
            var r = new TestShape();
            var s = new Solid(SolidOp.Difference, l,r);
            var ray = new Ray(new Point(0, 0, -5), new Vector(0, 1, 0));
            var xs = s.Intersects(ray);
            // Child not tested so SavedRay remains default.
            l.SavedRay.Should().Be(new Ray());
            r.SavedRay.Should().Be(new Ray());
        }

        [Fact]
        public void IntersectTestChildrenIfRayHitsBounds()
        {
            var l = new TestShape();
            var r = new TestShape();
            var s = new Solid(SolidOp.Difference, l, r);
            var ray = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = s.Intersects(ray);
            // Child tested so SavedRay in not default.
            l.SavedRay.Should().NotBe(new Ray());
            r.SavedRay.Should().NotBe(new Ray());
        }

        [Fact]
        public void DivideSubdividesChildren()
        {
            var s1 = new Sphere();
            s1.SetTransform(Transforms.Translate(-1.5f,0,0));
            var s2 = new Sphere();
            s2.SetTransform(Transforms.Translate(1.5f, 0, 0));
            var left = new Group(s1,s2);
            var s3 = new Sphere();
            s3.SetTransform(Transforms.Translate(0, 0, -1.5f));
            var s4 = new Sphere();
            s4.SetTransform(Transforms.Translate(0, 0, 1.5f));
            var right = new Group(s3,s4);
            var s = new Solid(SolidOp.Difference, left, right);
            s.Divide(1);
            var lg = (Group) s.Left;
            var rg = (Group) s.Right;
            var lsg1 = (Group) lg.Children[0];
            lsg1.Children.Should().Contain(s1);
            var lsg2 = (Group)lg.Children[1];
            lsg2.Children.Should().Contain(s2);
            var rsg1 = (Group)rg.Children[0];
            rsg1.Children.Should().Contain(s3);
            var rsg2 = (Group)rg.Children[1];
            rsg2.Children.Should().Contain(s4);
        }
    }
}