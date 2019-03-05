using System;

namespace Octans
{
    public struct Color : IEquatable<Color>
    {
        private const double Epsilon = 0.00001;

        public double Red;
        public double Green;
        public double Blue;

        public Color(double red, double green, double blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color Add(Color c) => new Color(Red + c.Red, Green + c.Green, Blue + c.Blue);

        public Color Subtract(Color c) => new Color(Red - c.Red, Green - c.Green, Blue - c.Blue);

        public Color Scale(double scalar) => new Color(Red * scalar, Green * scalar, Blue * scalar);

        public Color Divide(double scalar) => new Color(Red / scalar, Green / scalar, Blue / scalar);

        public Color Negate() => new Color(-Red, -Blue, -Green);

        public double Magnitude() => Math.Sqrt(Red * Red + Green * Green + Blue * Blue);

        public Color Normalize()
        {
            var m = Magnitude();
            return new Color(Red / m, Green / m, Blue / m);
        }

        public static Color HadamardProduct(Color c1, Color c2)
        {
            return new Color(c1.Red*c2.Red, c1.Green*c2.Green, c1.Blue*c2.Blue);
        }

        private static bool Equal(double a, double b) => Math.Abs(a - b) < Epsilon;

        public bool Equals(Color other) =>
            Equal(Red, other.Red) && Equal(Green, other.Green) && Equal(Blue, other.Blue);

        public override bool Equals(object obj) => !(obj is null) && obj is Color other && Equals(other);

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

        public static Color operator +(Color left, Color right) => left.Add(right);

        public static Color operator -(Color left, Color right) => left.Subtract(right);

        public static Color operator *(Color c, double scalar) => c.Scale(scalar);

        public static Color operator *(double scalar, Color c) => c.Scale(scalar);

        public static Color operator *(Color c1, Color c2) => HadamardProduct(c1,c2);

        public static Color operator /(Color c, double scalar) => c.Divide(scalar);

        public static Color operator -(Color c) => c.Negate();

        public static bool operator ==(Color left, Color right) => left.Equals(right);

        public static bool operator !=(Color left, Color right) => !left.Equals(right);

        public static Color Create(double red, double green, double blue) => new Color(red, green, blue);
    }
}