using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class TransformsTests
    {
        [Fact]
        public void TranslatePoint()
        {
            var t = Transforms.Translate(5, -3, 2);
            var p = Point.Create(-3, 4, 5);
            (t * p).Should().BeEquivalentTo(Point.Create(2, 1, 7));
        }

        [Fact]
        public void TranslateInversePoint()
        {
            var t = Transforms.Translate(5, -3, 2);
            var p = Point.Create(-3, 4, 5);
            (Matrix.Inverse(t) * p).Should().BeEquivalentTo(Point.Create(-8, 7, 3));
        }

        [Fact]
        public void TranslationDoesNotAffectVectors()
        {
            var t = Transforms.Translate(5, -3, 2);
            var v = Vector.Create(-3, 4, 5);
            (t*v).Should().BeEquivalentTo(v);
        }

        [Fact]
        public void ScalePoint()
        {
            var t = Transforms.Scale(2, 3, 4);
            var p = Point.Create(-4, 6, 8);
            (t * p).Should().BeEquivalentTo(Point.Create(-8, 18, 32));
        }

        [Fact]
        public void ScaleVector()
        {
            var t = Transforms.Scale(2, 3, 4);
            var v = Vector.Create(-4, 6, 8);
            (t * v).Should().BeEquivalentTo(Vector.Create(-8, 18, 32));
        }

        [Fact]
        public void ScaleInverseVector()
        {
            var t = Transforms.Scale(2, 3, 4);
            var v = Vector.Create(-4, 6, 8);
            (Matrix.Inverse(t) * v).Should().BeEquivalentTo(Vector.Create(-2, 2, 2));
        }

        [Fact]
        public void ReflectPoint()
        {
            var t = Transforms.Scale(-1, 1, 1);
            var p = Point.Create(2, 3, 4);
            (t * p).Should().BeEquivalentTo(Point.Create(-2, 3, 4));
        }

        [Fact]
        public void RotatePointAroundX()
        {
            var p = Point.Create(0, 1, 0);
            var halfQuarter = Transforms.RotateX(Math.PI / 4);
            var fullQuarter = Transforms.RotateX(Math.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(Point.Create(0, Math.Sqrt(2) / 2, Math.Sqrt(2) / 2));
            (fullQuarter * p).Should().BeEquivalentTo(Point.Create(0, 0, 1));
        }

        [Fact]
        public void RotateInversePointAroundX()
        {
            var p = Point.Create(0, 1, 0);
            var halfQuarter = Transforms.RotateX(Math.PI / 4);
            (Matrix.Inverse(halfQuarter) * p).Should().BeEquivalentTo(Point.Create(0, Math.Sqrt(2) / 2, -Math.Sqrt(2) / 2));
        }


        [Fact]
        public void RotatePointAroundY()
        {
            var p = Point.Create(0, 0, 1);
            var halfQuarter = Transforms.RotateY(Math.PI / 4);
            var fullQuarter = Transforms.RotateY(Math.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(Point.Create(Math.Sqrt(2) / 2,0, Math.Sqrt(2) / 2));
            (fullQuarter * p).Should().BeEquivalentTo(Point.Create(1, 0, 0));
        }

        [Fact]
        public void RotatePointAroundZ()
        {
            var p = Point.Create(0, 1, 0);
            var halfQuarter = Transforms.RotateZ(Math.PI / 4);
            var fullQuarter = Transforms.RotateZ(Math.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(Point.Create(-Math.Sqrt(2) / 2, Math.Sqrt(2) / 2, 0));
            (fullQuarter * p).Should().BeEquivalentTo(Point.Create(-1, 0, 0));
        }

        [Fact]
        public void ShearXInProportionToZ()
        {
            var t = Transforms.Shear(0, 1, 0, 0, 0, 0);
            var p = Point.Create(2, 3, 4);
            (t*p).Should().BeEquivalentTo(Point.Create(6,3,4));
        }

        [Fact]
        public void ShearYInProportionToX()
        {
            var t = Transforms.Shear(0, 0, 1, 0, 0, 0);
            var p = Point.Create(2, 3, 4);
            (t * p).Should().BeEquivalentTo(Point.Create(2, 5, 4));
        }

        [Fact]
        public void ShearYInProportionToZ()
        {
            var t = Transforms.Shear(0, 0, 0, 1, 0, 0);
            var p = Point.Create(2, 3, 4);
            (t * p).Should().BeEquivalentTo(Point.Create(2, 7, 4));
        }

        [Fact]
        public void ShearZInProportionToX()
        {
            var t = Transforms.Shear(0, 0, 0, 0, 1, 0);
            var p = Point.Create(2, 3, 4);
            (t * p).Should().BeEquivalentTo(Point.Create(2, 3, 6));
        }

        [Fact]
        public void ShearZInProportionToY()
        {
            var t = Transforms.Shear(0, 0, 0, 0, 0, 1);
            var p = Point.Create(2, 3, 4);
            (t * p).Should().BeEquivalentTo(Point.Create(2, 3, 7));
        }

        [Fact]
        public void SequenceTransforms()
        {
            var p = Point.Create(1, 0, 1);
            var a = Transforms.RotateX(Math.PI / 2);
            var b = Transforms.Scale(5, 5, 5);
            var c = Transforms.Translate(10, 5, 7);
            var p2 = a * p;
            var p3 = b * p2;
            var p4 = c * p3;
            p4.Should().BeEquivalentTo(Point.Create(15,0,7));
        }

        [Fact]
        public void ChainInReverseOrder()
        {
            var p = Point.Create(1, 0, 1);
            var a = Transforms.RotateX(Math.PI / 2);
            var b = Transforms.Scale(5, 5, 5);
            var c = Transforms.Translate(10, 5, 7);
            var t = c * b * a;
            (t*p).Should().BeEquivalentTo(Point.Create(15, 0, 7));
        }

        [Fact]
        public void FluentChaining()
        {
            var p = Point.Create(1, 0, 1);
            var t = Matrix.Identity.RotateX(Math.PI / 2).Scale(5, 5, 5).Translate(10, 5, 7);
            (t * p).Should().BeEquivalentTo(Point.Create(15, 0, 7));
        }
    }
}