using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class NormalTests
    {
        [Fact]
        public void SubtractingTwoNormals()
        {
            var n1 = new Normal(3, 2, 1);
            var n2 = new Normal(5, 6, 7);
            (n1 - n2).Should().BeEquivalentTo(new Normal(-2, -4, -6));
        }

        [Fact]
        public void Negate()
        {
            var a = new Normal(1, -2, 3);
            (-a).Should().BeEquivalentTo(new Normal(-1, 2, -3));
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var a = new Normal(1, -2, 3);
            (a * 3.5f).Should().BeEquivalentTo(new Normal(3.5f, -7, 10.5f));
        }

        [Fact]
        public void MultiplyingByFraction()
        {
            var a = new Normal(1, -2, 3);
            (a * 0.5f).Should().BeEquivalentTo(new Normal(0.5f, -1, 1.5f));
        }

        [Fact]
        public void DividingByScalar()
        {
            var a = new Normal(1, -2, 3);
            (a / 2).Should().BeEquivalentTo(new Normal(0.5f, -1, 1.5f));
        }

        [Fact]
        public void Magnitude()
        {
            var n = new Normal(1, 0, 0);
            n.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            n = new Normal(0, 1, 0);
            n.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            n = new Normal(1, 2, 3);
            n.Magnitude().Should().BeApproximately(System.MathF.Sqrt(14), 0.0001f);

            n = new Normal(-1, -2, -3);
            n.Magnitude().Should().BeApproximately(System.MathF.Sqrt(14), 0.0001f);
        }

        [Fact]
        public void Normalize()
        {
            var n = new Normal(4, 0, 0);
            n.Normalize().Should().BeEquivalentTo(new Normal(1, 0, 0));

            n = new Normal(1, 2, 3);
            n.Normalize().Should().BeEquivalentTo(new Normal(0.26726f, 0.53452f, 0.80178f));

            n.Normalize().Magnitude().Should().BeApproximately(1.0f, 0.0001f);
        }

        [Fact]
        public void DotProduct()
        {
            var a = new Normal(1, 2, 3);
            var b = new Normal(2, 3, 4);
            Normal.Dot(a, b).Should().BeApproximately(20f, 0.00001f);
            (a % b).Should().BeApproximately(20f, 0.00001f);
        }

        [Fact]
        public void AbsDotProduct()
        {
            var a = new Normal(1, 2, 3);
            var b = new Normal(2, 3, 4);
            Normal.AbsDot(a, b).Should().BeApproximately(20f, 0.00001f);
            b = new Normal(-2, 3, -4);
            Normal.Dot(a, b).Should().BeApproximately(-8f, 0.00001f);
            Normal.AbsDot(a, b).Should().BeApproximately(8f, 0.00001f);
        }

        [Fact]
        public void Reflect45DegVector()
        {
            var v = new Normal(1, -1, 0);
            var n = new Normal(0, 1, 0);
            var r = v.Reflect(n);
            r.Should().BeEquivalentTo(new Normal(1, 1, 0));
        }

        [Fact]
        public void ReflectOffSlantedSurface()
        {
            var v = new Normal(0, -1, 0);
            var n = new Normal(System.MathF.Sqrt(2f) / 2f, System.MathF.Sqrt(2f) / 2f, 0);
            var r = v.Reflect(n);
            r.Should().BeEquivalentTo(new Normal(1, 0, 0));
        }

        [Fact]
        public void AbsoluteValue()
        {
            var n = new Normal(0, -1, -2);
            var abs = Normal.Abs(in n);
            abs.X.Should().Be(0f);
            abs.Y.Should().Be(1f);
            abs.Z.Should().Be(2f);
        }

        [Fact]
        public void MaxComponent()
        {
            var n = new Normal(0, 3, -2);
            Normal.Max(n).Should().Be(3);
            n = new Normal(-1, -3, -2);
            Normal.Max(n).Should().Be(-1);
        }

        [Fact]
        public void FaceForward()
        {
          var n = new Normal(1,2,-3);
          var v = new Vector(0,0,1);
          var fn = Normal.FaceForward(n, v);
          fn.Should().Be(new Normal(-1, -2, 3));
        }

        [Fact]
        public void CanExplicitlyConvertBetweenVector()
        {
            var v = new Vector(1,2,3);
            var n = (Normal) v;
            var v1 = (Vector) n;
            v1.Should().Be(v);
        }
    }
}