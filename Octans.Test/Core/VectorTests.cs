using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class VectorTests
    {
        [Fact]
        public void SubtractingTwoVectors()
        {
            var v1 = new Vector(3, 2, 1);
            var v2 = new Vector(5, 6, 7);
            (v1 - v2).Should().BeEquivalentTo(new Vector(-2, -4, -6));
        }

        [Fact]
        public void SubtractingVectorFromZeroVector()
        {
            var zero = new Vector(0, 0, 0);
            var v = new Vector(1, -2, 3);
            (zero - v).Should().BeEquivalentTo(new Vector(-1, 2, -3));
        }

        [Fact]
        public void Negate()
        {
            var a = new Vector(1, -2, 3);
            (-a).Should().BeEquivalentTo(new Vector(-1, 2, -3));
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var a = new Vector(1, -2, 3);
            (a * 3.5f).Should().BeEquivalentTo(new Vector(3.5f, -7, 10.5f));
        }

        [Fact]
        public void MultiplyingByFraction()
        {
            var a = new Vector(1, -2, 3);
            (a * 0.5f).Should().BeEquivalentTo(new Vector(0.5f, -1, 1.5f));
        }

        [Fact]
        public void DividingByScalar()
        {
            var a = new Vector(1, -2, 3);
            (a / 2).Should().BeEquivalentTo(new Vector(0.5f, -1, 1.5f));
        }

        [Fact]
        public void Magnitude()
        {
            var v = new Vector(1, 0, 0);
            v.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            v = new Vector(0, 1, 0);
            v.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            v = new Vector(1, 2, 3);
            v.Magnitude().Should().BeApproximately(MathF.Sqrt(14), 0.0001f);

            v = new Vector(-1, -2, -3);
            v.Magnitude().Should().BeApproximately(MathF.Sqrt(14), 0.0001f);
        }

        [Fact]
        public void Normalize()
        {
            var v = new Vector(4, 0, 0);
            v.Normalize().Should().BeEquivalentTo(new Vector(1, 0, 0));

            v = new Vector(1, 2, 3);
            v.Normalize().Should().BeEquivalentTo(new Vector(0.26726f, 0.53452f, 0.80178f));

            v.Normalize().Magnitude().Should().BeApproximately(1.0f, 0.0001f);
        }

        [Fact]
        public void DotProduct()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(2, 3, 4);
            Vector.Dot(a, b).Should().BeApproximately(20f, 0.00001f);
        }

        [Fact]
        public void AbsDotProduct()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(2, 3, 4);
            Vector.AbsDot(a, b).Should().BeApproximately(20f, 0.00001f);
            b = new Vector(-2, 3, -4);
            Vector.Dot(a, b).Should().BeApproximately(-8f, 0.00001f);
            Vector.AbsDot(a, b).Should().BeApproximately(8f, 0.00001f);
        }

        [Fact]
        public void CrossProduct()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(2, 3, 4);
            Vector.Cross(a, b).Should().BeEquivalentTo(new Vector(-1, 2, -1));
            Vector.Cross(b, a).Should().BeEquivalentTo(new Vector(1, -2, 1));
        }

        [Fact]
        public void Reflect45DegVector()
        {
            var v = new Vector(1, -1, 0);
            var n = new Vector(0, 1, 0);
            var r = v.Reflect(n);
            r.Should().BeEquivalentTo(new Vector(1, 1, 0));
        }

        [Fact]
        public void ReflectOffSlantedSurface()
        {
            var v = new Vector(0, -1, 0);
            var n = new Vector(MathF.Sqrt(2f) / 2f, MathF.Sqrt(2f) / 2f, 0);
            var r = v.Reflect(n);
            r.Should().BeEquivalentTo(new Vector(1, 0, 0));
        }

        [Fact]
        public void AbsReturnsAbsoluteValuesInXYZ()
        {
            var v = new Vector(0, -1, -2);
            var abs = Vector.Abs(in v);
            abs.X.Should().Be(0f);
            abs.Y.Should().Be(1f);
            abs.Z.Should().Be(2f);
        }

        [Fact]
        public void MaxReturnGreatestValueInXYOrZ()
        {
            var v = new Vector(0, 3, -2);
            Vector.Max(v).Should().Be(3);
            v = new Vector(-1, -3, -2);
            Vector.Max(v).Should().Be(-1);
        }
    }
}