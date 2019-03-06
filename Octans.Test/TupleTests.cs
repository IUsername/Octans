using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class TupleTests
    {
        [Fact]
        public void WOfOneIsPoint()
        {
            var a = new Tuple(4.3f, -4.2f, 3.1f, 1.0f);
            a.X.Should().Be(4.3f);
            a.Y.Should().Be(-4.2f);
            a.Z.Should().Be(3.1f);
            a.W.Should().Be(1.0f);
            a.IsPoint().Should().BeTrue();
            a.IsVector().Should().BeFalse();
        }

        [Fact]
        public void WOfZeroIsVector()
        {
            var a = new Tuple(4.3f, -4.2f, 3.1f, 0.0f);
            a.X.Should().Be(4.3f);
            a.Y.Should().Be(-4.2f);
            a.Z.Should().Be(3.1f);
            a.W.Should().Be(0.0f);
            a.IsPoint().Should().BeFalse();
            a.IsVector().Should().BeTrue();
        }

        [Fact]
        public void PointCreatesTupleWithWOfOne()
        {
            var p = Point.Create(4, -4, 3);
            p.Should().BeEquivalentTo(new Tuple(4, -4, 3, 1));
        }

        [Fact]
        public void VectorCreatesTupleWithWOfZero()
        {
            var p = Vector.Create(4, -4, 3);
            p.Should().BeEquivalentTo(new Tuple(4, -4, 3, 0));
        }

        [Fact]
        public void CanAddTuples()
        {
            var a1 = new Tuple(3, -2, 5, 1);
            var a2 = new Tuple(-2, 3, 1, 0);
            (a1 + a2).Should().BeEquivalentTo(new Tuple(1, 1, 6, 1));
        }

        [Fact]
        public void SubtractingTwoPoints()
        {
            var p1 = Point.Create(3, 2, 1);
            var p2 = Point.Create(5, 6, 7);
            (p1 - p2).Should().BeEquivalentTo(Vector.Create(-2, -4, -6));
        }

        [Fact]
        public void SubtractingVectorFromPoint()
        {
            var p = Point.Create(3, 2, 1);
            var v = Vector.Create(5, 6, 7);
            (p - v).Should().BeEquivalentTo(Point.Create(-2, -4, -6));
        }

        [Fact]
        public void SubtractingTwoVectors()
        {
            var v1 = Vector.Create(3, 2, 1);
            var v2 = Vector.Create(5, 6, 7);
            (v1 - v2).Should().BeEquivalentTo(Vector.Create(-2, -4, -6));
        }

        [Fact]
        public void SubtractingVectorFromZeroVector()
        {
            var zero = Vector.Create(0, 0, 0);
            var v = Vector.Create(1, -2, 3);
            (zero - v).Should().BeEquivalentTo(Vector.Create(-1, 2, -3));
        }

        [Fact]
        public void NegateTuple()
        {
            var a = new Tuple(1, -2, 3, -4);
            (-a).Should().BeEquivalentTo(new Tuple(-1, 2, -3, 4));
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var a = new Tuple(1, -2, 3, -4);
            (a * 3.5f).Should().BeEquivalentTo(new Tuple(3.5f, -7, 10.5f, -14));
        }

        [Fact]
        public void MultiplyingByFraction()
        {
            var a = new Tuple(1, -2, 3, -4);
            (a * 0.5f).Should().BeEquivalentTo(new Tuple(0.5f, -1, 1.5f, -2));
        }

        [Fact]
        public void DividingByScalar()
        {
            var a = new Tuple(1, -2, 3, -4);
            (a / 2).Should().BeEquivalentTo(new Tuple(0.5f, -1, 1.5f, -2));
        }

        [Fact]
        public void Magnitude()
        {
            var v = Vector.Create(1, 0, 0);
            v.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            v = Vector.Create(0, 1, 0);
            v.Magnitude().Should().BeApproximately(1.0f, 0.0001f);

            v = Vector.Create(1, 2, 3);
            v.Magnitude().Should().BeApproximately(MathF.Sqrt(14), 0.0001f);

            v = Vector.Create(-1, -2, -3);
            v.Magnitude().Should().BeApproximately(MathF.Sqrt(14), 0.0001f);
        }

        [Fact]
        public void Normalize()
        {
            var v = Vector.Create(4, 0, 0);
            v.Normalize().Should().BeEquivalentTo(Vector.Create(1, 0, 0));

            v = Vector.Create(1, 2, 3);
            v.Normalize().Should().BeEquivalentTo(Vector.Create(0.26726f, 0.53452f, 0.80178f));

            v.Normalize().Magnitude().Should().BeApproximately(1.0f, 0.0001f);
        }

        [Fact]
        public void DotProduct()
        {
            var a = Vector.Create(1, 2, 3);
            var b = Vector.Create(2, 3, 4);
            Vector.Dot(a, b).Should().BeApproximately(20f, 0.00001f);
        }

        [Fact]
        public void CrossProduct()
        {
            var a = Vector.Create(1, 2, 3);
            var b = Vector.Create(2, 3, 4);
            Vector.Cross(a, b).Should().BeEquivalentTo(Vector.Create(-1, 2, -1));
            Vector.Cross(b, a).Should().BeEquivalentTo(Vector.Create(1, -2, 1));
        }
    }
}