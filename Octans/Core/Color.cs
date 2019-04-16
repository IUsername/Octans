using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans
{
    public readonly struct Color : IEquatable<Color>
    {
        private const float Epsilon = 0.001f;

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
        public Color Divide(float scalar)
        {
            var inv = 1f / scalar;
            return new Color(Red * inv, Green * inv, Blue * inv);
        }

        [Pure]
        public Color Pow(float exp) => new Color(System.MathF.Pow(Red, exp), System.MathF.Pow(Green, exp), System.MathF.Pow(Blue, exp));

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

        public static Color operator ^(Color c, float exp) => c.Pow(exp);

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

        //[Pure]
        //internal static bool IsWithinTolerance(in Color a, in Color b, float tolerance)
        //{
        //    var diff = a - b;
        //    return MathF.Abs(diff.Red) < tolerance && MathF.Abs(diff.Green) < tolerance && MathF.Abs(diff.Blue) < tolerance;
        //}

        [Pure]
        public static float PerceptiveColorDelta(in Color a, in Color b) =>
            System.MathF.Sqrt(PerceptiveColorDeltaSqr(in a, in b)) / 3f;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float PerceptiveColorDeltaSqr(in Color a, in Color b)
        {
            const float invGamma = 1 / 2.2f;
            var aS = a ^ invGamma;
            var bS = b ^ invGamma;
            //var aS = a;
            //var bS = b;
            var rAvg = (aS.Red + bS.Red) / 2f;
            var dR = aS.Red - bS.Red;
            var dG = aS.Green - bS.Green;
            var dB = aS.Blue - bS.Blue;
            var rFac = 2f + rAvg / 1f;
            var bFac = 2 + (1f - rAvg) / 1f;
            return rFac * dR * dR + 4f * dG * dG + bFac * dB * dB;
        }

        [Pure]
        public static bool IsWithinPerceptiveDelta(in Color a, in Color b, float delta)
        {
            var cD = PerceptiveColorDeltaSqr(in a, in b);
            return cD < delta * delta * 9f;
        }

        //[Pure]
        //public static (float y, float u, float v) ToYUV(in Color color)
        //{
        //    var v = new Vector(color.Red,color.Green, color.Blue);
        //    var r = YUVTransform * v;
        //    return (r.X, r.Y, r.Z);
        //}

        //private static Matrix YUVTransform =
        //    Matrix.Square(0.299f, 0.587f, 0.114f, 0f, -0.14713f, -0.28886f, 0.436f, 0f, 0.615f, -0.51499f, -0.10001f, 0f, 0f, 0f, 0f, 1f);

        [Pure]
        public static Color Lerp(in Color a, in Color b, float t) => (1 - t) * a + t * b;
    }
}