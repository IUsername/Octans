using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Color : IEquatable<Color>
    {
        private const float Epsilon = 0.0001f;

        public readonly float Red;
        public readonly float Green;
        public readonly float Blue;

        public Color(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        [Pure]
        public Color Add(in Color c) => new Color(Red + c.Red, Green + c.Green, Blue + c.Blue);

        [Pure]
        public Color Subtract(in Color c) => new Color(Red - c.Red, Green - c.Green, Blue - c.Blue);

        [Pure]
        public Color Scale(float scalar) => new Color(Red * scalar, Green * scalar, Blue * scalar);

        [Pure]
        public Color Divide(float scalar) => new Color(Red / scalar, Green / scalar, Blue / scalar);

        [Pure]
        public Color Negate() => new Color(-Red, -Blue, -Green);

        [Pure]
        public static Color HadamardProduct(in Color c1, in Color c2) =>
            new Color(c1.Red * c2.Red, c1.Green * c2.Green, c1.Blue * c2.Blue);

        public bool Equals(Color other) =>
            Check.Within(Red, other.Red, Epsilon)
            && Check.Within(Green, other.Green, Epsilon)
            && Check.Within(Blue, other.Blue, Epsilon);

        public static Color operator +(Color left, Color right) => left.Add(in right);

        public static Color operator -(Color left, Color right) => left.Subtract(in right);

        public static Color operator *(Color c, float scalar) => c.Scale(scalar);

        public static Color operator *(float scalar, Color c) => c.Scale(scalar);

        public static Color operator *(Color c1, Color c2) => HadamardProduct(in c1, in c2);

        public static Color operator /(Color c, float scalar) => c.Divide(scalar);

        public static Color operator -(Color c) => c.Negate();

        public static bool operator ==(Color left, Color right) => left.Equals(right);

        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Color other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Red.GetHashCode();
                hashCode = (hashCode * 397) ^ Green.GetHashCode();
                hashCode = (hashCode * 397) ^ Blue.GetHashCode();
                return hashCode;
            }
        }

        [Pure]
        internal static bool IsWithinTolerance(in Color a, in Color b, float tolerance)
        {
            var diff = a - b;
            return MathF.Abs(diff.Red) < tolerance && MathF.Abs(diff.Green) < tolerance && MathF.Abs(diff.Blue) < tolerance;
        }
    }
}