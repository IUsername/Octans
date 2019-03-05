using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class Tuple
    {
        [Fact]
        public void WOfOneIsPoint()
        {
            var a = new Octans.Tuple(4.3, -4.2, 3.1, 1.0);
            a.X.Should().Be(4.3);
            a.Y.Should().Be(-4.2);
            a.Z.Should().Be(3.1);
            a.W.Should().Be(1.0);
            a.IsPoint().Should().BeTrue();
            a.IsVector().Should().BeFalse();
        }

        [Fact]
        public void WOfZeroIsVector()
        {
            var a = new Octans.Tuple(4.3, -4.2, 3.1, 0.0);
            a.X.Should().Be(4.3);
            a.Y.Should().Be(-4.2);
            a.Z.Should().Be(3.1);
            a.W.Should().Be(0.0);
            a.IsPoint().Should().BeFalse();
            a.IsVector().Should().BeTrue();
        }

        [Fact]
        public void PointCreatesTupleWithWOfOne()
        {
            var p = Point.Create(4, -4, 3);
            p.Should().BeEquivalentTo(new Octans.Tuple(4, -4, 3, 1));
        }

        [Fact]
        public void VectorCreatesTupleWithWOfZero()
        {
            var p = Vector.Create(4, -4, 3);
            p.Should().BeEquivalentTo(new Octans.Tuple(4, -4, 3, 0));
        }

        [Fact]
        public void CanAddTuples()
        {
            var a1 = new Octans.Tuple(3, -2, 5, 1);
            var a2 = new Octans.Tuple(-2, 3, 1, 0);
            (a1 + a2).Should().BeEquivalentTo(new Octans.Tuple(1, 1, 6, 1));
        }

        [Fact]
        public void SubtractingTwoPoints()
        {
            var p1 = Point.Create(3, 2, 1);
            var p2 = Point.Create(5, 6, 7);
            (p1-p2).Should().BeEquivalentTo(Vector.Create(-2,-4,-6));
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
            (v1-v2).Should().BeEquivalentTo(Vector.Create(-2,-4,-6));
        }

        [Fact]
        public void SubtractingVectorFromZeroVector()
        {
            var zero = Vector.Create(0, 0, 0);
            var v = Vector.Create(1, -2, 3);
            (zero-v).Should().BeEquivalentTo(Vector.Create(-1,2,-3));
        }

        [Fact]
        public void NegateTuple()
        {
            var a = new Octans.Tuple(1,-2,3,-4);
            (-a).Should().BeEquivalentTo(new Octans.Tuple(-1,2,-3,4));
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var a = new Octans.Tuple(1, -2, 3, -4);
            (a * 3.5).Should().BeEquivalentTo(new Octans.Tuple(3.5,-7,10.5,-14));
        }

        [Fact]
        public void MultiplyingByFraction()
        {
            var a = new Octans.Tuple(1, -2, 3, -4);
            (a * 0.5).Should().BeEquivalentTo(new Octans.Tuple(0.5, -1, 1.5, -2));
        }

        [Fact]
        public void DividingByScalar()
        {
            var a = new Octans.Tuple(1, -2, 3, -4);
            (a / 2).Should().BeEquivalentTo(new Octans.Tuple(0.5, -1, 1.5, -2));
        }

        [Fact]
        public void Magnitude()
        {
            var v = Vector.Create(1, 0, 0);
            v.Magnitude().Should().BeApproximately(1.0,0.0001);

            v = Vector.Create(0, 1, 0);
            v.Magnitude().Should().BeApproximately(1.0, 0.0001);

            v = Vector.Create(1, 2, 3);
            v.Magnitude().Should().BeApproximately(Math.Sqrt(14), 0.0001);

            v = Vector.Create(-1, -2, -3);
            v.Magnitude().Should().BeApproximately(Math.Sqrt(14), 0.0001);
        }

        [Fact]
        public void Normalize()
        {
            var v = Vector.Create(4, 0, 0);
            v.Normalize().Should().BeEquivalentTo(Vector.Create(1,0,0));

            v = Vector.Create(1, 2, 3);
            v.Normalize().Should().BeEquivalentTo(Vector.Create(0.26726, 0.53452, 0.80178));

            v.Normalize().Magnitude().Should().BeApproximately(1.0, 0.0001);
        }

    }
}