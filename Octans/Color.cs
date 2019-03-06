using System;

namespace Octans
{
    public readonly struct Color : IEquatable<Color>
    {
        private const float Epsilon = 0.00001f;

        public readonly float Red;
        public readonly float Green;
        public readonly float Blue;

        public Color(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color(Color other)
        {
            this = other;
        }

        public Color Add(Color c) => new Color(Red + c.Red, Green + c.Green, Blue + c.Blue);

        public Color Subtract(Color c) => new Color(Red - c.Red, Green - c.Green, Blue - c.Blue);

        public Color Scale(float scalar) => new Color(Red * scalar, Green * scalar, Blue * scalar);

        public Color Divide(float scalar) => new Color(Red / scalar, Green / scalar, Blue / scalar);

        public Color Negate() => new Color(-Red, -Blue, -Green);

        public static Color HadamardProduct(Color c1, Color c2) =>
            new Color(c1.Red * c2.Red, c1.Green * c2.Green, c1.Blue * c2.Blue);

        private static bool Equal(float a, float b) => Math.Abs(a - b) < Epsilon;

        public bool Equals(Color other) =>
            Equal(Red, other.Red) && Equal(Green, other.Green) && Equal(Blue, other.Blue);

        public static Color operator +(Color left, Color right) => left.Add(right);

        public static Color operator -(Color left, Color right) => left.Subtract(right);

        public static Color operator *(Color c, float scalar) => c.Scale(scalar);

        public static Color operator *(float scalar, Color c) => c.Scale(scalar);

        public static Color operator *(Color c1, Color c2) => HadamardProduct(c1, c2);

        public static Color operator /(Color c, float scalar) => c.Divide(scalar);

        public static Color operator -(Color c) => c.Negate();

        public static bool operator ==(Color left, Color right) => left.Equals(right);

        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public static Color RGB(float red, float green, float blue) => new Color(red, green, blue);

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
    }
}