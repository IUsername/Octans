using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public readonly struct Point : IEquatable<Point>
    {
        private const float Epsilon = 0.0001f;

        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Point(float x, float y, float z)
        {
            Debug.Assert(!float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z));
            X = x;
            Y = y;
            Z = z;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public Point Add(in Vector t) => new Point(X + t.X, Y + t.Y, Z + t.Z);

        public Vector Subtract(in Point t) => new Vector(X - t.X, Y - t.Y, Z - t.Z);

        public Point Subtract(in Vector t) => new Point(X - t.X, Y - t.Y, Z - t.Z);

        private Vector Multiply(in Vector vector) => new Vector(X * vector.X, Y * vector.Y, Z * vector.Z);

        public Point Negate() => new Point(-X, -Y, -Z);

        public Point Scale(float scalar) => new Point(X * scalar, Y * scalar, Z * scalar);

        public Point Divide(float scalar)
        {
            var inv = 1f / scalar;
            return Scale(inv);
        }

        public bool Equals(Point other) =>
            Check.Within(X, other.X, Epsilon)
            && Check.Within(Y, other.Y, Epsilon)
            && Check.Within(Z, other.Z, Epsilon);

        public override bool Equals(object obj) => !(obj is null) && obj is Point other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        [Pure]
        public static float Distance(in Point a, in Point b) => (a - b).Magnitude();

        [Pure]
        public static float DistanceSqr(in Point a, in Point b) => (a - b).MagSqr();

        [Pure]
        public static Point operator +(Point left, Vector right) => left.Add(in right);

        [Pure]
        public static Vector operator -(Point left, Point right) => left.Subtract(in right);

        [Pure]
        public static Point operator -(Point left, Vector right) => left.Subtract(in right);

        [Pure]
        public static Point operator *(Point t, float scalar) => t.Scale(scalar);

        [Pure]
        public static Point operator *(float scalar, Point t) => t.Scale(scalar);

        [Pure]
        public static Vector operator *(Point t, Vector v) => t.Multiply(in v);

        [Pure]
        public static Point operator /(Point t, float scalar) => t.Divide(scalar);

        [Pure]
        public static Point operator -(Point t) => t.Negate();

        [Pure]
        public static explicit operator Vector(Point p) => new Vector(p.X, p.Y, p.Z);

        [Pure]
        public static bool operator ==(Point left, Point right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public static Point Zero = new Point(0f, 0f, 0f);

        [Pure]
        public static Point Abs(in Point t) =>
            new Point(System.MathF.Abs(t.X), System.MathF.Abs(t.Y), System.MathF.Abs(t.Z));

        [Pure]
        public static float Max(in Point t) => System.MathF.Max(t.X, System.MathF.Max(t.Y, t.Z));
    }
}