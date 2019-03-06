using System;

namespace Octans
{
    public readonly struct Point : IEquatable<Point>
    {
        private const float Epsilon = 0.00001f;

        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public Point(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Point(float x, float y, float z) : this(x, y, z, 1.0f) { }

        public Point Add(Vector t) => new Point(X + t.X, Y + t.Y, Z + t.Z, W + t.W);

        public Vector Subtract(Point t) => new Vector(X - t.X, Y - t.Y, Z - t.Z);

        public Point Subtract(Vector t) => new Point(X - t.X, Y - t.Y, Z - t.Z, W);

        public Point Negate() => new Point(-X, -Y, -Z, -W);

        public Point Scale(float scalar) => new Point(X * scalar, Y * scalar, Z * scalar, W * scalar);

        public Point Divide(float scalar) => new Point(X / scalar, Y / scalar, Z / scalar, W / scalar);

        public bool Equals(Point other) =>
            Check.Within(X, other.X, Epsilon) 
            && Check.Within(Y, other.Y, Epsilon) 
            && Check.Within(Z, other.Z, Epsilon) 
            && Check.Within(W, other.W, Epsilon);

        public override bool Equals(object obj) => !(obj is null) && obj is Point other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        public static Point operator +(Point left, Vector right) => left.Add(right);

        public static Vector operator -(Point left, Point right) => left.Subtract(right);

        public static Point operator -(Point left, Vector right) => left.Subtract(right);

        public static Point operator *(Point t, float scalar) => t.Scale(scalar);

        public static Point operator *(float scalar, Point t) => t.Scale(scalar);

        public static Point operator /(Point t, float scalar) => t.Divide(scalar);

        public static Point operator -(Point t) => t.Negate();

        public static bool operator ==(Point left, Point right) => left.Equals(right);

        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public static Point Zero = new Point(0f, 0f, 0f);
    }
}