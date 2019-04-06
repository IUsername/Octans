using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Vector : IEquatable<Vector>
    {
        private const float Epsilon = 0.0001f;

        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        private Vector(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector(float x, float y, float z) : this(x, y, z, 0.0f)
        {
        }

        public Vector ZeroW() => new Vector(X, Y, Z);

        public Vector Add(in Vector t) => new Vector(X + t.X, Y + t.Y, Z + t.Z, W + t.W);

        public Vector Subtract(in Vector t) => new Vector(X - t.X, Y - t.Y, Z - t.Z, W - t.W);

        public Vector Negate() => new Vector(-X, -Y, -Z, -W);

        public Vector Scale(float scalar) => new Vector(X * scalar, Y * scalar, Z * scalar, W * scalar);

        public Vector Divide(float scalar)
        {
            var inv = 1f / scalar;
            return Scale(inv);
        }

        public Vector Fraction(float scalar) => new Vector(scalar / X, scalar / Y, scalar / Z, scalar / W);

        public float Magnitude() => MathF.Sqrt(MagSqr());

        public float MagSqr() => X * X + Y * Y + Z * Z + W * W;

        public Vector Normalize()
        {
            var m = Magnitude();
            return Divide(m);
        }

        public Vector Reflect(in Vector normal) => Reflect(in this, in normal);

        public bool Equals(Vector other) =>
            Check.Within(X, other.X, Epsilon)
            && Check.Within(Y, other.Y, Epsilon)
            && Check.Within(Z, other.Z, Epsilon)
            && Check.Within(W, other.W, Epsilon);

        public override bool Equals(object obj) => !(obj is null) && obj is Vector other && Equals(other);

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

        [Pure]
        public static Vector operator +(Vector left, Vector right) => left.Add(in right);

        [Pure]
        public static Vector operator -(Vector left, Vector right) => left.Subtract(in right);

        [Pure]
        public static Vector operator *(Vector t, float scalar) => t.Scale(scalar);

        [Pure]
        public static Vector operator *(float scalar, Vector t) => t.Scale(scalar);

        [Pure]
        public static Vector operator /(Vector t, float scalar) => t.Divide(scalar);

        [Pure]
        public static Vector operator /(float scalar, Vector t) => t.Fraction(scalar);

        [Pure]
        public static Vector operator -(Vector t) => t.Negate();

        [Pure]
        public static bool operator ==(Vector left, Vector right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Vector left, Vector right) => !left.Equals(right);

        [Pure]
        public static float operator %(Vector left, Vector right) => Dot(in left, in right);

        [Pure]
        public static float Dot(in Vector a, in Vector b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

        [Pure]
        public static float AbsDot(in Vector a, in Vector b) => MathF.Abs(Dot(in a, in b));

        [Pure]
        public static Vector Cross(in Vector a, in Vector b)
        {
            double aX = a.X, aY = a.Y, aZ = a.Z;
            double bX = b.X, bY = b.Y, bZ = b.Z;
            return new Vector((float) (aY * bZ - aZ * bY),
                              (float) (aZ * bX - aX * bZ),
                              (float) (aX * bY - aY * bX));
        }

        [Pure]
        public static Vector Reflect(in Vector @in, in Vector normal) => @in - normal * 2f * Dot(in @in, in normal);

        [Pure]
        public static Vector Abs(in Vector t) =>
            new Vector(MathF.Abs(t.X), MathF.Abs(t.Y), MathF.Abs(t.Z), MathF.Abs(t.W));

        [Pure]
        public static float Max(in Vector t) => MathF.Max(t.X, MathF.Max(t.Y, t.Z));
    }
}