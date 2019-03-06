using System;

namespace Octans
{
    public struct Matrix : IEquatable<Matrix>
    {
        private const double Epsilon = 0.00001;

        public int Rows { get; }
        public int Columns { get; }

        private readonly double[,] _data;

        public Matrix(params double[][] values)
        {
            Rows = values.Length;
            Columns = values[0].Length;

            _data = new double[Rows, Columns];
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    _data[row, col] = values[row][col];
                }
            }
        }

        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _data = new double[Rows, Columns];
        }

        public double this[int row, int col]
        {
            get => _data[row, col];
            private set => _data[row, col] = value;
        }

        public double[][] ToArray()
        {
            var arr = new double[Rows][];
            for (var row = 0; row < Rows; row++)
            {
                var cur = new double[Columns];
                for (var col = 0; col < Columns; col++)
                {
                    cur[col] = this[row, col];
                }

                arr[row] = cur;
            }

            return arr;
        }

        public Matrix Transpose()
        {
            var m = new Matrix(Columns, Rows);
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    m[col, row] = this[row, col];
                }
            }

            return m;
        }

        public bool Equals(Matrix other) => Rows == other.Rows && Columns == other.Columns && ValuesEqual(this, other);

        private static bool Equal(double a, double b) => Math.Abs(a - b) < Epsilon;

        private bool ValuesEqual(Matrix a, Matrix b)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    if (!Equal(a[row, col], b[row, col]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Matrix other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _data != null ? _data.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Rows;
                hashCode = (hashCode * 397) ^ Columns;
                return hashCode;
            }
        }

        public static Matrix Identity = new Matrix(new[] {1.0, 0, 0, 0},
                                                    new[] {0.0, 1, 0, 0},
                                                    new[] {0.0, 0, 1, 0},
                                                    new[] {0.0, 0, 0, 1});

        public static Matrix Square(params double[] values)
        {
            var len = values.Length;
            int size;
            switch (len)
            {
                case 16:
                    size = 4;
                    break;
                case 9:
                    size = 3;
                    break;
                case 4:
                    size = 2;
                    break;
                default:
                    size = (int) Math.Sqrt(len);
                    break;
            }

            var arr = new double[size][];
            var k = 0;
            for (var row = 0; row < size; row++)
            {
                var cur = new double[size];
                for (var col = 0; col < size; col++)
                {
                    cur[col] = values[k++];
                }

                arr[row] = cur;
            }

            return new Matrix(arr);
        }

        private static Matrix ToMatrix(Tuple t) => new Matrix(new[] {t.X}, new[] {t.Y}, new[] {t.Z}, new[] {t.W});

        private static Tuple ToTuple(Matrix m) => new Tuple(m[0, 0], m[1, 0], m[2, 0], m[3, 0]);

        private static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
            {
                throw new InvalidOperationException("Matrices do not have the correct shapes for multiplication.");
            }

            var m = new Matrix(a.Rows, b.Columns);
            for (var r = 0; r < a.Rows; r++)
            {
                for (var c = 0; c < b.Columns; c++)
                {
                    var temp = 0.0;
                    for (var k = 0; k < a.Columns; k++)
                    {
                        temp += a[r, k] * b[k, c];
                    }

                    m[r, c] = temp;
                }
            }

            return m;
        }

        public static double Determinant(Matrix m)
        {
            if (m.Columns == 2 && m.Rows == 2)
            {
               return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
            }

            var det = 0.0;
            for (var c = 0; c < m.Columns; c++)
            {
                det += m[0, c] * Cofactor(m, 0, c);
            }

            return det;
        }

        public static Matrix Submatrix(Matrix m, int row, int column)
        {
            var arr = new double[m.Rows - 1][];
            var rr = 0;
            for (var r = 0; r < m.Rows; r++)
            {
                if (r == row)
                {
                    continue;
                }

                var cc = 0;
                var cur = new double[m.Columns - 1];
                for (var c = 0; c < m.Columns; c++)
                {
                    if (c == column)
                    {
                        continue;
                    }

                    cur[cc++] = m[r, c];
                }

                arr[rr++] = cur;
            }

            return new Matrix(arr);
        }

        public static double Minor(Matrix m, int row, int column)
        {
            var s = Submatrix(m, row, column);
            return Determinant(s);
        }

        public static double Cofactor(Matrix m, int row, int column)
        {
            var n = Minor(m, row, column);
            return (row + column) % 2 == 0 ? n : -n;
        }

        public static bool IsInvertible(Matrix m)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return Determinant(m) != 0.0;
        }

        public static Matrix Inverse(Matrix m)
        {
            var det = Determinant(m);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (det == 0.0)
            {
                throw new InvalidOperationException("Matrix is not invertible.");
            }

            var m2 = new Matrix(m.Columns, m.Rows);
            for (var r = 0; r < m.Rows; r++)
            {
                for (var c = 0; c < m.Columns; c++)
                {
                    var cf = Cofactor(m, r, c);
                    // Transpose.
                    m2[c, r] = cf / det;
                }
            }

            return m2;
        }

        public static bool operator ==(Matrix left, Matrix right) => left.Equals(right);

        public static bool operator !=(Matrix left, Matrix right) => !left.Equals(right);

        public static Matrix operator *(Matrix left, Matrix right) => Multiply(left, right);

        public static Tuple operator *(Matrix left, Tuple right) => ToTuple(Multiply(left, ToMatrix(right)));
    }
}