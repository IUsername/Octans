using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class MatrixTest
    {
        [Fact]
        public void Create4x4()
        {
            var m = new Matrix(
                new[] {1.0, 2, 3, 4},
                new[] {5.5, 6.5, 7.5, 8.5},
                new[] {9.0, 10, 11, 12},
                new[] {13.5, 14.5, 15.5, 16.5});
            m[0, 0].Should().BeApproximately(1, 0.00001);
            m[0, 3].Should().BeApproximately(4, 0.00001);
            m[1, 0].Should().BeApproximately(5.5, 0.00001);
            m[1, 2].Should().BeApproximately(7.5, 0.00001);
            m[2, 2].Should().BeApproximately(11, 0.00001);
            m[3, 0].Should().BeApproximately(13.5, 0.00001);
            m[3, 2].Should().BeApproximately(15.5, 0.00001);
        }

        [Fact]
        public void Create2x2()
        {
            var m = new Matrix(new[] {-3.0, 5.0}, new[] {1.0, -2.0});
            m[0, 0].Should().BeApproximately(-3, 0.00001);
            m[0, 1].Should().BeApproximately(5, 0.00001);
            m[1, 0].Should().BeApproximately(1, 0.00001);
            m[1, 1].Should().BeApproximately(-2, 0.00001);
        }

        [Fact]
        public void Create3x3()
        {
            var m = Matrix.Square(-3, 5, 0, 1, -2, -7, 0, 1, 1);
            m[0, 0].Should().BeApproximately(-3, 0.00001);
            m[1, 1].Should().BeApproximately(-2, 0.00001);
            m[2, 2].Should().BeApproximately(1, 0.00001);
        }

        [Fact]
        public void IdenticalEquality()
        {
            var a = Matrix.Square(1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2);
            var b = Matrix.Square(1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2);
            a.Should().BeEquivalentTo(b);
        }

        [Fact]
        public void NonEquality()
        {
            var a = Matrix.Square(1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2);
            var b = Matrix.Square(2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2, 1);
            (a == b).Should().BeFalse();
        }

        [Fact]
        public void MultiplyMatrices()
        {
            var a = Matrix.Square(1, 2, 3, 4, 5, 6, 7, 8, 9, 8, 7, 6, 5, 4, 3, 2);
            var b = Matrix.Square(-2, 1, 2, 3, 3, 2, 1, -1, 4, 3, 6, 5, 1, 2, 7, 8);
            (a * b).Should()
                   .BeEquivalentTo(Matrix.Square(20, 22, 50, 48, 44, 54, 114, 108, 40, 58, 110, 102, 16, 26, 46, 42));
        }

        [Fact]
        public void MultipleMatrixAndTuple()
        {
            var a = Matrix.Square(1, 2, 3, 4, 2, 4, 4, 2, 8, 6, 4, 1, 0, 0, 0, 1);
            var b = new Tuple(1, 2, 3, 1);
            (a * b).Should().BeEquivalentTo(new Tuple(18, 24, 33, 1));
        }

        [Fact]
        public void MatrixMultipliedByIdentityIsMatrix()
        {
            var a = Matrix.Square(1, 2, 3, 4, 1, 2, 4, 8, 2, 4, 8, 16, 4, 8, 16, 32);
            (a * Matrix.Identity).Should().BeEquivalentTo(a);
        }

        [Fact]
        public void TupleMultipliedByIdentityMatrixIsTuple()
        {
            var a = new Tuple(1, 2, 3, 4);
            (Matrix.Identity * a).Should().BeEquivalentTo(a);
        }

        [Fact]
        public void CanTranspose()
        {
            var m = Matrix.Square(0, 9, 3, 0, 9, 8, 0, 8, 1, 8, 5, 3, 0, 0, 5, 8);
            m.Transpose().Should().BeEquivalentTo(Matrix.Square(0, 9, 1, 0, 9, 8, 8, 0, 3, 0, 5, 5, 0, 8, 3, 8));
        }

        [Fact]
        public void DeterminantOf2x2()
        {
            var m = Matrix.Square(1, 5, -3, 2);
            Matrix.Determinant(m).Should().Be(17);
        }

        [Fact]
        public void Submatrix3x3()
        {
            var a = Matrix.Square(1, 5, 0, -3, 2, 7, 0, 6, -3);
            Matrix.Submatrix(a, 0, 2).Should().BeEquivalentTo(Matrix.Square(-3, 2, 0, 6));
        }

        [Fact]
        public void Submatrix4x4()
        {
            var a = Matrix.Square(-6, 1, 1, 6, -8, 5, 8, 6, -1, 0, 8, 2, -7, 1, -1, 1);
            Matrix.Submatrix(a, 2, 1).Should().BeEquivalentTo(Matrix.Square(-6, 1, 6, -8, 8, 6, -7, -1, 1));
        }

        [Fact]
        public void MinorOf3x3()
        {
            var a = Matrix.Square(3, 5, 0, 2, -1, -7, 6, -1, 5);
            Matrix.Minor(a, 1, 0).Should().Be(25);
        }

        [Fact]
        public void CofactorOf3x3()
        {
            var a = Matrix.Square(3, 5, 0, 2, -1, -7, 6, -1, 5);
            Matrix.Cofactor(a, 0, 0).Should().Be(-12);
            Matrix.Cofactor(a, 1, 0).Should().Be(-25);
        }

        [Fact]
        public void DeterminantOf3x3()
        {
            var m = Matrix.Square(1, 2,6,-5,8,-4,2,6,4);
            Matrix.Cofactor(m, 0, 0).Should().Be(56);
            Matrix.Cofactor(m, 0, 1).Should().Be(12);
            Matrix.Cofactor(m, 0, 2).Should().Be(-46);
            Matrix.Determinant(m).Should().Be(-196);
        }

        [Fact]
        public void DeterminantOf4x4()
        {
            var m = Matrix.Square(-2,-8,3,5,-3,1,7,3,1,2,-9,6,-6,7,7,-9);
            Matrix.Cofactor(m, 0, 0).Should().Be(690);
            Matrix.Cofactor(m, 0, 1).Should().Be(447);
            Matrix.Cofactor(m, 0, 2).Should().Be(210);
            Matrix.Cofactor(m, 0, 3).Should().Be(51);
            Matrix.Determinant(m).Should().Be(-4071);
        }

        [Fact]
        public void IsInvertible()
        {
            var m = Matrix.Square(6, 4, 4, 4, 5, 5, 7, 9, 4, -9, 3, -7, 9, 1, 7, -6);
            Matrix.IsInvertible(m).Should().BeTrue();
        }

        [Fact]
        public void IsNotInvertible()
        {
            var m = Matrix.Square(-4,2,-2,-3,9,6,2,6,0,-5,1,-5,0,0,0,0);
            Matrix.IsInvertible(m).Should().BeFalse();
        }

        [Fact]
        public void MatrixInverse()
        {
            var a = Matrix.Square(8, -5, 9, 2, 7, 5, 6, 1, -6, 0, 9, 6, -3, 0, -9, -4);
            var invA = Matrix.Inverse(a);
            invA.Should()
                .BeEquivalentTo(
                    Matrix.Square(-0.15385, -0.15385, -0.28205, -0.53846,
                                  -0.07692, 0.12308, 0.02564, 0.03077, 
                                  0.35897, 0.35897, 0.43590, 0.92308, 
                                  -0.69231, -0.69231,-0.76923, -1.92308));

            var b = Matrix.Square(9, 3, 0, 9, -5, -2, -6, -3, -4, 9, 6, 4, -7, 6, 6, 2);
            var invB = Matrix.Inverse(b);
            invB.Should()
                .BeEquivalentTo(
                    Matrix.Square(-0.04074, -0.07778, 0.14444, -0.22222,
                                  -0.07778, 0.03333, 0.36667, -0.33333,
                                  -0.02901, -0.14630, -0.10926, 0.12963,
                                  0.17778, 0.06667, -0.26667, 0.33333));
        }

        [Fact]
        public void MultiplyProductByInverse()
        {
            var a = Matrix.Square(3, -9, 7, 3, 3, -8, 2, -9, -4, 4, 4, 1, -6, 5, -1, 1);
            var b = Matrix.Square(8,2,2,2,3,-1,7,0,7,0,5,4,6,-2,0,5);
            var c = a * b;
            (c * Matrix.Inverse(b)).Should().BeEquivalentTo(a);
        }
    }
}