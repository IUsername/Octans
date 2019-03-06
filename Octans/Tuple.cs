using System;

namespace Octans
{
    public readonly struct Tuple : IEquatable<Tuple>
    {
        private const float Epsilon = 0.00001f;

        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        public Tuple(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool IsPoint() => 1.0.Equals(W);

        public bool IsVector() => 0.0.Equals(W);

        public static Tuple Point(float x, float y, float z) => new Tuple(x, y, z, 1.0f);

        public static Tuple Vector(float x, float y, float z) => new Tuple(x, y, z, 0.0f);

        public Tuple Add(Tuple t)
        {
            return new Tuple(X + t.X, Y + t.Y, Z + t.Z, W + t.W);
        }

        public Tuple Subtract(Tuple t)
        {
            return new Tuple(X - t.X, Y - t.Y, Z - t.Z, W - t.W);
        }

        public Tuple Negate()
        {
            return new Tuple(-X,-Y,-Z,-W);
        }

        public Tuple Scale(float scalar)
        {
            return new Tuple(X*scalar,Y*scalar,Z*scalar,W*scalar);
        }

        public Tuple Divide(float scalar)
        {
            return new Tuple(X / scalar, Y / scalar, Z / scalar, W / scalar);
        }

        public float Magnitude()
        {
            return MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        public Tuple Normalize()
        {
            var m = Magnitude();
            return new Tuple(X / m, Y / m, Z / m, W / m);
        }

        private static bool Equal(float a, float b) => Math.Abs(a - b) < Epsilon;
        
        public bool Equals(Tuple other) =>
            Equal(X, other.X) && Equal(Y, other.Y) && Equal(Z, other.Z) && Equal(W, other.W);

        public override bool Equals(object obj) => !(obj is null) && obj is Tuple other && Equals(other);

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

        public static Tuple operator +(Tuple left, Tuple right) => left.Add(right);

        public static Tuple operator -(Tuple left, Tuple right) => left.Subtract(right);

        public static Tuple operator *(Tuple t, float scalar) => t.Scale(scalar);

        public static Tuple operator *(float scalar, Tuple t) => t.Scale(scalar);

        public static Tuple operator /(Tuple t, float scalar) => t.Divide(scalar);

        public static Tuple operator -(Tuple t) => t.Negate();

        public static bool operator ==(Tuple left, Tuple right) => left.Equals(right);

        public static bool operator !=(Tuple left, Tuple right) => !left.Equals(right);
    }

    public static class Point
    {
        public static Tuple Create(float x, float y, float z) => Tuple.Point(x, y, z);
    }

    public static class Vector
    {
        public static Tuple Create(float x, float y, float z) => Tuple.Vector(x, y, z);

        public static double Dot(Tuple a, Tuple b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        public static Tuple Cross(Tuple a, Tuple b)
        {
            return Create(a.Y * b.Z - a.Z * b.Y,
                          a.Z * b.X - a.X * b.Z,
                          a.X * b.Y - a.Y * b.X);
        }
    }
}