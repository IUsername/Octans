using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public readonly struct Matrix : IEquatable<Matrix>
    {
        private const float Epsilon = 0.00001f;

        public readonly int Rows;
        public readonly int Columns;
        private readonly bool _isIdentity;

        private readonly float[,] _data;

        public Matrix(params float[][] values)
        {
            Rows = values.Length;
            Columns = values[0].Length;

            _data = new float[Rows, Columns];
            var isIdentity = true;
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    _data[row, col] = values[row][col];
                    if (!isIdentity)
                    {
                        continue;
                    }

                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    if (row == col)
                    {
                        isIdentity = _data[row, col] == 1.0f;
                    }
                    else
                    {
                        isIdentity = _data[row, col] == 0.0f;
                    }
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                }
            }

            _isIdentity = isIdentity;
        }

        private Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _data = new float[Rows, Columns];
            _isIdentity = false;
        }

        public float this[int row, int col] => _data[row, col];

        //public float[][] ToArray()
        //{
        //    var arr = new float[Rows][];
        //    for (var row = 0; row < Rows; row++)
        //    {
        //        var cur = new float[Columns];
        //        for (var col = 0; col < Columns; col++)
        //        {
        //            cur[col] = this[row, col];
        //        }

        //        arr[row] = cur;
        //    }

        //    return arr;
        //}

        public Matrix Transpose()
        {
            if (_isIdentity)
            {
                return this;
            }

            var m = new Matrix(Columns, Rows);
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Columns; col++)
                {
                    m._data[col, row] = this[row, col];
                }
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

        public static Matrix Identity = new Matrix(new[] {1.0f, 0, 0, 0},
                                                   new[] {0.0f, 1, 0, 0},
                                                   new[] {0.0f, 0, 1, 0},
                                                   new[] {0.0f, 0, 0, 1});

        public static Matrix Square(params float[] values)
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

            var arr = new float[size][];
            var k = 0;
            for (var row = 0; row < size; row++)
            {
                var cur = new float[size];
                for (var col = 0; col < size; col++)
                {
                    cur[col] = values[k++];
                }

                arr[row] = cur;
            }

            return new Matrix(arr);
        }

        [Pure]
        private static Point ToPoint(in Matrix m) => new Point(m[0, 0], m[1, 0], m[2, 0], m[3, 0]);

        [Pure]
        private static Vector ToVector(in Matrix m) => new Vector(m[0, 0], m[1, 0], m[2, 0]);

        [Pure]
        private static Matrix Multiply(in Matrix a, in Matrix b)
        {
            if (a._isIdentity)
            {
                return b;
            }

            if (b._isIdentity)
            {
                return a;
            }

            if (a.Columns != b.Rows)
            {
                throw new InvalidOperationException("Matrices do not have the correct shapes for multiplication.");
            }

            var m = new Matrix(a.Rows, b.Columns);
            for (var r = 0; r < a.Rows; r++)
            {
                for (var c = 0; c < b.Columns; c++)
                {
                    var temp = 0.0f;
                    for (var k = 0; k < a.Columns; k++)
                    {
                        temp += a[r, k] * b[k, c];
                    }

                    m._data[r, c] = temp;
                }
            }

            return m;
        }

        //[Pure]
        //private static Point MultiplyPoint(in Matrix a, in Point b)
        //{
        //    if (a._isIdentity)
        //    {
        //        return b;
        //    }

        //    if (a.Columns != 4)
        //    {
        //        throw new InvalidOperationException("Matrices do not have the correct shapes for multiplication.");
        //    }

        //    var m = new Matrix(a.Rows, b.Columns);
        //    for (var r = 0; r < a.Rows; r++)
        //    {
        //        for (var c = 0; c < 4; c++)
        //        {
        //            var temp = 0.0f;
        //            for (var k = 0; k < a.Columns; k++)
        //            {
        //                temp += a[r, k] * b[k, c];
        //            }

        //            m._data[r, c] = temp;
        //        }
        //    }
        //    return new Point(x,y,z,w);
        //}


        [Pure]
        public static float Determinant(in Matrix m)
        {
            if (m.Columns == 2 && m.Rows == 2)
            {
                return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
            }

            var det = 0.0f;
            for (var c = 0; c < m.Columns; c++)
            {
                det += m[0, c] * Cofactor(m, 0, c);
            }

            return det;
        }

        [Pure]
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
        public static Matrix Inverse(in Matrix m)
        {
            if (m._isIdentity)
            {
                return m;
            }

            var det = Determinant(m);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (det == 0.0f)
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
                    m2._data[c, r] = cf / det;
                }
            }

            return m2;
        }

        public Matrix Inverse() => Inverse(this);

        private static readonly ObjectPool<Matrix> ColMatPool = new ObjectPool<Matrix>(() => new Matrix(new[] { 0f }, new[] { 0f }, new[] { 0f }, new[] { 0f }));
        private static readonly ObjectPool<Matrix> TransformMatPool = new ObjectPool<Matrix>(() => new Matrix(4, 4));

        private static Matrix ColMatFromPool(float x, float y, float z, float w)
        {
            var m = ColMatPool.GetObject();
            m._data[0, 0] = x;
            m._data[1, 0] = y;
            m._data[2, 0] = z;
            m._data[3, 0] = w;
            return m;
        }

        private static void ReturnColMat(Matrix m)
        {
            ColMatPool.PutObject(m);
        }

        private static void ReturnTransformMatPool(Matrix m)
        {
            TransformMatPool.PutObject(m);
        }

        [Pure]
        private static Matrix MultiplyTransformPool(in Matrix a, in Matrix b)
        {
            if (a.Columns != 4 && a.Rows != 4)
            {
                throw new InvalidOperationException("Pool only holds 4x4 matrices");
            }
            if (a.Columns != b.Rows)
            {
                throw new InvalidOperationException("Matrices do not have the correct shapes for multiplication.");
            }

            var m = TransformMatPool.GetObject();
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < b.Columns; j++)
                {
                    m._data[i, j] = 0f;
                    m._data[i, j] += a[i, 0] * b[0, j];
                    m._data[i, j] += a[i, 1] * b[1, j];
                    m._data[i, j] += a[i, 2] * b[2, j];
                    m._data[i, j] += a[i, 3] * b[3, j];
                }
            }

            return m;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Point Multiply4x4Point(in Matrix a, in Point b)
        {

            //if (a.Columns != 4 && a.Rows != 4)
            //{
            //    throw new InvalidOperationException("Pool only holds 4x4 matrices");
            //}

            var x = a[0, 0] * b.X + a[0, 1] * b.Y + a[0, 2] * b.Z + a[0, 3] * b.W;
            var y = a[1, 0] * b.X + a[1, 1] * b.Y + a[1, 2] * b.Z + a[1, 3] * b.W;
            var z = a[2, 0] * b.X + a[2, 1] * b.Y + a[2, 2] * b.Z + a[2, 3] * b.W;
            var w = a[3, 0] * b.X + a[3, 1] * b.Y + a[3, 2] * b.Z + a[3, 3] * b.W;

            return new Point(x, y, z, w);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector Multiply4x4Vector(in Matrix a, in Vector b)
        {

            //if (a.Columns != 4 && a.Rows != 4)
            //{
            //    throw new InvalidOperationException("Pool only holds 4x4 matrices");
            //}

            var x = a[0, 0] * b.X + a[0, 1] * b.Y + a[0, 2] * b.Z + a[0, 3] * b.W;
            var y = a[1, 0] * b.X + a[1, 1] * b.Y + a[1, 2] * b.Z + a[1, 3] * b.W;
            var z = a[2, 0] * b.X + a[2, 1] * b.Y + a[2, 2] * b.Z + a[2, 3] * b.W;

            return new Vector(x, y, z);
        }

        [Pure]
        private static Point OpMultiplyPoint(in Matrix left, in Point right)
        {
                return Multiply4x4Point(in left, in right);

            //if (left._isIdentity) return right;
            //var rMat = ColMatFromPool(right.X, right.Y, right.Z, right.W);
            //var m = MultiplyTransformPool(in left, in rMat);
            //var p = ToPoint(in m);
            //ReturnColMat(rMat);
            //ReturnTransformMatPool(m);
            //return p;
        }

        [Pure]
        private static Vector OpMultiplyVector(in Matrix left, in Vector right)
        {
            return Multiply4x4Vector(in left, in right);

            //if (left._isIdentity) return right;
            //var rMat = ColMatFromPool(right.X, right.Y, right.Z, right.W);
            //var m = MultiplyTransformPool(in left, in rMat);
            //var v = ToVector(in m);
            //ReturnColMat(rMat);
            //ReturnTransformMatPool(m);
            //return v;
        }

        [Pure]
        public static bool operator ==(Matrix left, Matrix right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Matrix left, Matrix right) => !left.Equals(right);

        [Pure]
        public static Matrix operator *(Matrix left, Matrix right) => Multiply(in left, in right);

        [Pure]
        public static Point operator *(Matrix left, Point right) => OpMultiplyPoint(in left, in right);

        [Pure]
        public static Vector operator *(Matrix left, Vector right) => OpMultiplyVector(in left, in right);
    }
}