using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;

namespace Octans
{
    public readonly struct Matrix : IEquatable<Matrix>
    {
        private const float Epsilon = 0.00001f;

        public readonly int Rows;

        public readonly int Columns;

        private readonly float[,] _data;

        public Matrix(
            float m00,
            float m01,
            float m02,
            float m03,
            float m10,
            float m11,
            float m12,
            float m13,
            float m20,
            float m21,
            float m22,
            float m23,
            float m30,
            float m31,
            float m32,
            float m33)
        {
            Rows = 4;
            Columns = 4;
            _data = new float[4, 4];
            _data[0, 0] = m00;
            _data[0, 1] = m01;
            _data[0, 2] = m02;
            _data[0, 3] = m03;
            _data[1, 0] = m10;
            _data[1, 1] = m11;
            _data[1, 2] = m12;
            _data[1, 3] = m13;
            _data[2, 0] = m20;
            _data[2, 1] = m21;
            _data[2, 2] = m22;
            _data[2, 3] = m23;
            _data[3, 0] = m30;
            _data[3, 1] = m31;
            _data[3, 2] = m32;
            _data[3, 3] = m33;
        }

        private Matrix(params float[][] values)
        {
            Rows = values.Length;
            Columns = values[0].Length;
            _data = new float[Rows, Columns];
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    _data[row, col] = values[row][col];
                }
            }
        }

        private Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _data = new float[Rows, Columns];
        }

        public float this[int row, int col] => _data[row, col];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix Transpose()
        {
            var m = new Matrix(4, 4);
            for (var row = 0; row < Rows; row++)
            {
                m._data[0, row] = this[row, 0];
                m._data[1, row] = this[row, 1];
                m._data[2, row] = this[row, 2];
                m._data[3, row] = this[row, 3];
            }

            return m;
        }

        public bool Equals(Matrix other) =>
            Rows == other.Rows && Columns == other.Columns && ValuesEqual(in this, in other);

        [Pure]
        private bool ValuesEqual(in Matrix a, in Matrix b)
        {
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    if (!Check.Within(a[row, col], b[row, col], Epsilon))
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

        public static Matrix Identity = new Matrix(1, 0, 0, 0,
                                                   0, 1, 0, 0,
                                                   0, 0, 1, 0,
                                                   0, 0, 0, 1);


        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Matrix Multiply(in Matrix a, in Matrix b)
        {
            var m = new Matrix(4, 4);
            for (var r = 0; r < 4; r++)
            {
                for (var c = 0; c < 4; c++)
                {
                    ////m._data[r, c] = 0.0f;
                    //m._data[r, c] = a[r, 0] * b[0, c];
                    //m._data[r, c] += a[r, 1] * b[1, c];
                    //m._data[r, c] += a[r, 2] * b[2, c];
                    //m._data[r, c] += a[r, 3] * b[3, c];

                    m._data[r, c] = FusedMultiplyAdd(a[r, 1], b[1, c], a[r, 0] * b[0, c]);
                    m._data[r, c] = FusedMultiplyAdd(a[r, 2], b[2, c], m._data[r, c]);
                    m._data[r, c] = FusedMultiplyAdd(a[r, 3], b[3, c], m._data[r, c]);
                }
            }

            return m;
        }


        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Determinant(in Matrix m)
        {
            if (m.Columns == 2 && m.Rows == 2)
            {
                //return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
                return FusedMultiplyAdd(m[0, 0], m[1, 1], - m[0, 1] * m[1, 0]);
            }

            var det = 0.0f;
            for (var c = 0; c < m.Columns; c++)
            {
                //det += m[0, c] * Cofactor(m, 0, c);
                det = FusedMultiplyAdd(m[0, c], Cofactor(m, 0, c), det);
            }

            return det;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix Submatrix(in Matrix m, int row, int column)
        {
            var arr = new float[m.Rows - 1][];
            var rr = 0;
            for (var r = 0; r < m.Rows; r++)
            {
                if (r == row)
                {
                    continue;
                }

                var cc = 0;
                var cur = new float[m.Columns - 1];
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

        [Pure]
        public static float Minor(in Matrix m, int row, int column)
        {
            var s = Submatrix(m, row, column);
            return Determinant(s);
        }

        [Pure]
        public static float Cofactor(in Matrix m, int row, int column)
        {
            var n = Minor(m, row, column);
            return (row + column) % 2 == 0 ? n : -n;
        }

        [Pure]
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public static bool IsInvertible(in Matrix m) => Determinant(m) != 0.0f;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix Inverse(in Matrix m)
        {
            var det = Determinant(m);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (det == 0.0f)
            {
                throw new InvalidOperationException("Matrix is not invertible.");
            }

            var detInv = 1f / det;
            var m2 = new Matrix(m.Columns, m.Rows);
            for (var r = 0; r < m.Rows; r++)
            {
                for (var c = 0; c < m.Columns; c++)
                {
                    var cf = Cofactor(m, r, c);
                    // Transpose
                    m2._data[c, r] = cf * detInv;
                }
            }

            return m2;
        }

        public Matrix Inverse() => Inverse(this);

        [Pure]
        public static bool operator ==(Matrix left, Matrix right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Matrix left, Matrix right) => !left.Equals(right);

        [Pure]
        public static Matrix operator *(Matrix left, Matrix right) => Multiply(in left, in right);
    }
}