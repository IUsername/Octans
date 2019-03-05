using System;

namespace Octans
{
    public struct Tuple : IEquatable<Tuple>
    {
        private const double Epsilon = 0.00001;

        public double X;
        public double Y;
        public double Z;
        public double W;

        public Tuple(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool IsPoint() => W.Equals(1.0);

        public bool IsVector() => W.Equals(0.0);

        public static Tuple Point(double x, double y, double z) => new Tuple(x, y, z, 1.0);

        public static Tuple Vector(double x, double y, double z) => new Tuple(x, y, z, 0.0);

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

        public Tuple Scale(double scalar)
        {
            return new Tuple(X*scalar,Y*scalar,Z*scalar,W*scalar);
        }

        public Tuple Divide(double scalar)
        {
            return new Tuple(X / scalar, Y / scalar, Z / scalar, W / scalar);
        }

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        }

        public Tuple Normalize()
        {
            var m = Magnitude();
            return new Tuple(X / m, Y / m, Z / m, W / m);
        }

        private static bool Equal(double a, double b) => Math.Abs(a - b) < Epsilon;
        
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

        public static Tuple operator *(Tuple t, double scalar) => t.Scale(scalar);

        public static Tuple operator *(double scalar, Tuple t) => t.Scale(scalar);

        public static Tuple operator /(Tuple t, double scalar) => t.Divide(scalar);

        public static Tuple operator -(Tuple t) => t.Negate();

        public static bool operator ==(Tuple left, Tuple right) => left.Equals(right);

        public static bool operator !=(Tuple left, Tuple right) => !left.Equals(right);
    }

    public static class Point
    {
        public static Tuple Create(double x, double y, double z) => Tuple.Point(x, y, z);
    }

    public static class Vector
    {
        public static Tuple Create(double x, double y, double z) => Tuple.Vector(x, y, z);

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