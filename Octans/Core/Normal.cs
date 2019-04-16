using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Normal : IEquatable<Normal>
    {
        private const float Epsilon = 0.0001f;

        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Normal(float x, float y, float z) 
        {
            Debug.Assert(!float.IsNaN(x) && !float.IsNaN(y) && !float.IsNaN(z));
            X = x;
            Y = y;
            Z = z;
        }

        public Normal Add(in Normal t) => new Normal(X + t.X, Y + t.Y, Z + t.Z);

        public Normal Subtract(in Normal t) => new Normal(X - t.X, Y - t.Y, Z - t.Z);

        public Normal Negate() => new Normal(-X, -Y, -Z);

        public Normal Scale(float scalar) => new Normal(X * scalar, Y * scalar, Z * scalar);

        public Normal Divide(float scalar)
        {
            var inv = 1f / scalar;
            return Scale(inv);
        }

        public Normal Fraction(float scalar) => new Normal(scalar / X, scalar / Y, scalar / Z);

        public float Magnitude() => MathF.Sqrt(MagSqr());

        public float MagSqr() => X * X + Y * Y + Z * Z;

        public Normal Normalize()
        {
            var m = Magnitude();
            return Divide(m);
        }

        [Pure]
        public static Normal FaceForward(in Normal n, in Vector v)
        {
            return n % v < 0f ? -n : n;
        }

        public Normal Reflect(in Normal normal) => Reflect(in this, in normal);

        public bool Equals(Normal other) =>
            Check.Within(X, other.X, Epsilon)
            && Check.Within(Y, other.Y, Epsilon)
            && Check.Within(Z, other.Z, Epsilon);

        public override bool Equals(object obj) => !(obj is null) && obj is Normal other && Equals(other);

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
        public static Normal operator +(Normal left, Normal right) => left.Add(in right);

        [Pure]
        public static Normal operator -(Normal left, Normal right) => left.Subtract(in right);

        [Pure]
        public static Normal operator *(Normal t, float scalar) => t.Scale(scalar);

        [Pure]
        public static Normal operator *(float scalar, Normal t) => t.Scale(scalar);

        [Pure]
        public static Normal operator /(Normal t, float scalar) => t.Divide(scalar);

        [Pure]
        public static Normal operator /(float scalar, Normal t) => t.Fraction(scalar);

        [Pure]
        public static Normal operator -(Normal t) => t.Negate();

        [Pure]
        public static bool operator ==(Normal left, Normal right) => left.Equals(right);

        [Pure]
        public static bool operator !=(Normal left, Normal right) => !left.Equals(right);

        [Pure]
        public static float operator %(Normal left, Normal right) => Dot(in left, in right);

        [Pure]
        public static float operator %(Normal left, Vector right) => Dot(in left, in right);

        [Pure]
        public static float operator %(Vector left, Normal right) => Dot(in left, in right);

        [Pure]
        public static float Dot(in Normal a, in Normal b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [Pure]
        public static float Dot(in Normal a, in Vector b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [Pure]
        public static float Dot(in Vector a, in Normal b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        [Pure]
        public static float AbsDot(in Normal a, in Normal b) => MathF.Abs(Dot(in a, in b));

        [Pure]
        public static Normal Reflect(in Normal @in, in Normal normal) => @in - normal * 2f * Dot(in @in, in normal);

        [Pure]
        public static Normal Abs(in Normal t) =>
            new Normal(MathF.Abs(t.X), MathF.Abs(t.Y), MathF.Abs(t.Z));

        [Pure]
        public static float Max(in Normal t) => MathF.Max(t.X, MathF.Max(t.Y, t.Z));

        [Pure]
        public static explicit  operator Vector(in Normal n) => new Vector(n.X, n.Y, n.Z);

        [Pure]
        public static explicit operator Normal(in Vector v) => new Normal(v.X, v.Y, v.Z);

    }

    public static class Normals
    {
        public static Normal YPos = new Normal(0,1,0);
        public static Normal YNeg = new Normal(0,-1,0);
        public static Normal ZPos = new Normal(0,0,1);
    }
}