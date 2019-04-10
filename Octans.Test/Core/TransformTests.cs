using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class TransformTests
    {
        [Fact]
        public void TranslatePoint()
        {
            var t = Transform.Translate(5, -3, 2);
            var p = new Point(-3, 4, 5);
            (t * p).Should().BeEquivalentTo(new Point(2, 1, 7));
        }

        [Fact]
        public void TranslateInversePoint()
        {
            var t = Transform.Translate(5, -3, 2);
            var p = new Point(-3, 4, 5);
            (Transform.Invert(t) * p).Should().BeEquivalentTo(new Point(-8, 7, 3));
        }

        [Fact]
        public void TranslationDoesNotAffectVectors()
        {
            var t = Transform.Translate(5, -3, 2);
            var v = new Vector(-3, 4, 5);
            (t * v).Should().BeEquivalentTo(v);
        }

        [Fact]
        public void ScalePoint()
        {
            var t = Transform.Scale(2, 3, 4);
            var p = new Point(-4, 6, 8);
            (t * p).Should().BeEquivalentTo(new Point(-8, 18, 32));
        }

        [Fact]
        public void ScaleVector()
        {
            var t = Transform.Scale(2, 3, 4);
            var v = new Vector(-4, 6, 8);
            (t * v).Should().BeEquivalentTo(new Vector(-8, 18, 32));
        }

        [Fact]
        public void ScaleInverseVector()
        {
            var t = Transform.Scale(2, 3, 4);
            var v = new Vector(-4, 6, 8);
            (Transform.Invert(t) * v).Should().BeEquivalentTo(new Vector(-2, 2, 2));
        }

        [Fact]
        public void ReflectPoint()
        {
            var t = Transform.Scale(-1, 1, 1);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(-2, 3, 4));
        }

        [Fact]
        public void RotatePointAroundX()
        {
            var p = new Point(0, 1, 0);
            var halfQuarter = Transform.RotateX(MathF.PI / 4);
            var fullQuarter = Transform.RotateX(MathF.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(new Point(0, MathF.Sqrt(2) / 2, MathF.Sqrt(2) / 2));
            (fullQuarter * p).Should().BeEquivalentTo(new Point(0, 0, 1));
        }

        [Fact]
        public void RotateInversePointAroundX()
        {
            var p = new Point(0, 1, 0);
            var halfQuarter = Transform.RotateX(MathF.PI / 4);
            var inv = Transform.Invert(in halfQuarter);
            (inv * p)
                .Should()
                .BeEquivalentTo(new Point(0, MathF.Sqrt(2) / 2, -MathF.Sqrt(2) / 2));
        }


        [Fact]
        public void RotatePointAroundY()
        {
            var p = new Point(0, 0, 1);
            var halfQuarter = Transform.RotateY(MathF.PI / 4);
            var fullQuarter = Transform.RotateY(MathF.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(new Point(MathF.Sqrt(2) / 2, 0, MathF.Sqrt(2) / 2));
            (fullQuarter * p).Should().BeEquivalentTo(new Point(1, 0, 0));
        }

        [Fact]
        public void RotatePointAroundZ()
        {
            var p = new Point(0, 1, 0);
            var halfQuarter = Transform.RotateZ(MathF.PI / 4);
            var fullQuarter = Transform.RotateZ(MathF.PI / 2);
            (halfQuarter * p).Should().BeEquivalentTo(new Point(-MathF.Sqrt(2) / 2, MathF.Sqrt(2) / 2, 0));
            (fullQuarter * p).Should().BeEquivalentTo(new Point(-1, 0, 0));
        }

        [Fact]
        public void ShearXInProportionToZ()
        {
            var t = Transform.Shear(0, 1, 0, 0, 0, 0);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(6, 3, 4));
        }

        [Fact]
        public void ShearYInProportionToX()
        {
            var t = Transform.Shear(0, 0, 1, 0, 0, 0);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(2, 5, 4));
        }

        [Fact]
        public void ShearYInProportionToZ()
        {
            var t = Transform.Shear(0, 0, 0, 1, 0, 0);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(2, 7, 4));
        }

        [Fact]
        public void ShearZInProportionToX()
        {
            var t = Transform.Shear(0, 0, 0, 0, 1, 0);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(2, 3, 6));
        }

        [Fact]
        public void ShearZInProportionToY()
        {
            var t = Transform.Shear(0, 0, 0, 0, 0, 1);
            var p = new Point(2, 3, 4);
            (t * p).Should().BeEquivalentTo(new Point(2, 3, 7));
        }

        [Fact]
        public void SequenceTransforms()
        {
            var p = new Point(1, 0, 1);
            var a = Transform.RotateX(MathF.PI / 2);
            var b = Transform.Scale(5, 5, 5);
            var c = Transform.Translate(10, 5, 7);
            var p2 = a * p;
            var p3 = b * p2;
            var p4 = c * p3;
            p4.Should().BeEquivalentTo(new Point(15, 0, 7));
        }

        [Fact]
        public void ChainInReverseOrder()
        {
            var p = new Point(1, 0, 1);
            var a = Transform.RotateX(MathF.PI / 2);
            var b = Transform.Scale(5, 5, 5);
            var c = Transform.Translate(10, 5, 7);
            var t = c * b * a;
            (t * p).Should().BeEquivalentTo(new Point(15, 0, 7));
        }

        [Fact]
        public void FluentChaining()
        {
            var p = new Point(1, 0, 1);
            var t = Transform.RotateX(MathF.PI / 2).Scale(5, 5, 5).Translate(10, 5, 7);
            (t * p).Should().BeEquivalentTo(new Point(15, 0, 7));
        }

        [Fact]
        public void DefaultViewTransformIsIdentity()
        {
            var from = new Point(0, 0, 0);
            var to = new Point(0, 0, -1);
            var up = new Vector(0, 1, 0);
            var t = Transform.LookAt(from, to, up);
            t.Matrix.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void ViewTransformLookingInPositiveZ()
        {
            var from = new Point(0, 0, 0);
            var to = new Point(0, 0, 1);
            var up = new Vector(0, 1, 0);
            var t = Transform.LookAt(from, to, up);
            var p = new Point(0, 0, 0);
            (t * p).Should().Be(new Point(0,0,0));
        }

        [Fact]
        public void ViewTranslation()
        {
            var from = new Point(0, 0, 8);
            var to = new Point(0, 0, 0);
            var up = new Vector(0, 1, 0);
            var t = Transform.LookAt(from, to, up);
            var p = new Point(0, 0, 0);
            (t * p).Should().Be(new Point(0, 0, -8));
        }

        //[Fact]
        //public void ArbitraryView()
        //{
        //    var from = new Point(1, 3, 2);
        //    var to = new Point(4, -2, 8);
        //    var up = new Vector(1, 1, 0);
        //    var t = Transform.LookAt(from, to, up);
        //    t.Matrix.Should()
        //     .Be(Matrix.Square(
        //             -0.50709f, 0.50709f, 0.67612f, -2.36643f,
        //             0.76772f, 0.60609f, 0.12122f, -2.82843f,
        //             -0.35857f, 0.59761f, -0.71714f, 0.0000f,
        //             0.00000f, 0.00000f, 0.00000f, 1.0000f));
        //}
    }
}