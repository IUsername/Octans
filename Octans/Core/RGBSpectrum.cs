using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Numerics.Vector;

namespace Octans
{
    public class RGBSpectrum : IEquatable<RGBSpectrum>
    {
        private const int SpectrumSamples = 3;

        public static RGBSpectrum Black = new RGBSpectrum();
        private readonly Vector<float> _samples;

        public RGBSpectrum(float v)
        {
            var samples = new float[Vector<float>.Count];
            for (var i = 0; i < SpectrumSamples; i++)
            {
                samples[i] = v;
            }

            _samples = new Vector<float>(samples);
        }

        public RGBSpectrum()
        {
            _samples = Vector<float>.Zero;
        }

        private RGBSpectrum(Vector<float> samples)
        {
            _samples = samples;
        }

        [Pure]
        public float this[int index] => _samples[index];

        [Pure]
        public bool Equals(RGBSpectrum other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || _samples.Equals(other._samples);
        }

        [Pure]
        public bool IsBlack() => EqualsAll(_samples, Vector<float>.Zero);

        [Pure]
        public bool HasNaN()
        {
            for (var i = 0; i < SpectrumSamples; i++)
            {
                if (float.IsNaN(_samples[i]))
                {
                    return true;
                }
            }

            return false;
        }

        [Pure]
        public RGBSpectrum Add(in RGBSpectrum other)
        {
            var s = System.Numerics.Vector.Add(_samples, other._samples);
            return new RGBSpectrum(s);
        }

        [Pure]
        public RGBSpectrum Subtract(in RGBSpectrum other)
        {
            var s = System.Numerics.Vector.Subtract(_samples, other._samples);
            return new RGBSpectrum(s);
        }

        [Pure]
        public RGBSpectrum Multiply(in RGBSpectrum other)
        {
            var s = System.Numerics.Vector.Multiply(_samples, other._samples);
            return new RGBSpectrum(s);
        }

        [Pure]
        public RGBSpectrum Multiply(float scalar)
        {
            var s = System.Numerics.Vector.Multiply(_samples, scalar);
            return new RGBSpectrum(s);
        }

        [Pure]
        public RGBSpectrum Divide(in RGBSpectrum other)
        {
            // Using vector divide will always result in divide by zero.
            var s = new float[SpectrumSamples];
            for (var i = 0; i < SpectrumSamples; i++)
            {
                s[i] = _samples[i] / other._samples[i];
            }

            return new RGBSpectrum(new Vector<float>(s));
        }

        [Pure]
        public RGBSpectrum Divide(float scalar)
        {
            var inv = 1f / scalar;
            var s = System.Numerics.Vector.Multiply(_samples, inv);
            return new RGBSpectrum(s);
        }

        [Pure]
        public RGBSpectrum Sqrt()
        {
            var s = SquareRoot(_samples);
            return new RGBSpectrum(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public RGBSpectrum Pow(float y)
        {
            var s = new float[SpectrumSamples];
            for (var i = 0; i < SpectrumSamples; i++)
            {
                s[i] = System.MathF.Pow(_samples[i], y);
            }

            return new RGBSpectrum(new Vector<float>(s));
        }

        [Pure]
        public static RGBSpectrum Negate(in RGBSpectrum spectrum)
        {
            var s = System.Numerics.Vector.Negate(spectrum._samples);
            return new RGBSpectrum(s);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGBSpectrum Lerp(in RGBSpectrum s0, in RGBSpectrum s1, float t)
        {
            var s = System.Numerics.Vector.Add(
                System.Numerics.Vector.Multiply(s0._samples, 1f - t),
                System.Numerics.Vector.Multiply(s1._samples, t));
            return new RGBSpectrum(s);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGBSpectrum Clamp(in RGBSpectrum spectrum, float low, float high = float.PositiveInfinity)
        {
            var s = new float[SpectrumSamples];
            for (var i = 0; i < SpectrumSamples; i++)
            {
                s[i] = MathF.Clamp(low, high, spectrum._samples[i]);
            }

            return new RGBSpectrum(new Vector<float>(s));
        }

        [Pure]
        public static float MaxComponent(in RGBSpectrum spectrum)
        {
            var max = float.NegativeInfinity;
            for (var i = 0; i < SpectrumSamples; i++)
            {
                max = System.MathF.Max(spectrum._samples[i], max);
            }

            return max;
        }

        [Pure]
        public static Color ToColor(in RGBSpectrum spectrum) =>
            new Color(spectrum._samples[0], spectrum._samples[1], spectrum._samples[2]);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RGBSpectrum FromColor(in Color color)
        {
            var components = new float[Vector<float>.Count];
            components[0] = color.Red;
            components[1] = color.Green;
            components[2] = color.Blue;
            var s = new Vector<float>(components);
            return new RGBSpectrum(s);
        }

        [Pure]
        public static RGBSpectrum operator +(RGBSpectrum left, RGBSpectrum right) => left.Add(in right);

        [Pure]
        public static RGBSpectrum operator -(RGBSpectrum left, RGBSpectrum right) => left.Subtract(in right);

        [Pure]
        public static RGBSpectrum operator *(RGBSpectrum left, RGBSpectrum right) => left.Multiply(in right);

        [Pure]
        public static RGBSpectrum operator *(RGBSpectrum left, float right) => left.Multiply(right);

        [Pure]
        public static RGBSpectrum operator /(RGBSpectrum left, RGBSpectrum right) => left.Divide(in right);

        [Pure]
        public static RGBSpectrum operator /(RGBSpectrum left, float right) => left.Divide(right);

        [Pure]
        public static RGBSpectrum operator ^(RGBSpectrum s, float exp) => s.Pow(exp);

        [Pure]
        public static RGBSpectrum operator -(RGBSpectrum s) => Negate(in s);

        [Pure]
        public static bool Equals(RGBSpectrum x, RGBSpectrum y) => EqualsAll(x._samples, y._samples);

        [Pure]
        public static bool operator ==(RGBSpectrum left, RGBSpectrum right) => Equals(left, right);

        [Pure]
        public static bool operator !=(RGBSpectrum left, RGBSpectrum right) => !Equals(left, right);

        [Pure]
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((RGBSpectrum) obj);
        }

        [Pure]
        public override int GetHashCode() => _samples.GetHashCode();

        //[Pure]
        //public float[] ToXYZ()
        //{
        //    var xyz = new float[3];
        //    for (var i = 0; i < SpectrumSamples; i++)
        //    {
        //        xyz[0] += X.c[i] * _samples[i];
        //        xyz[1] += Y.c[i] * _samples[i];
        //        xyz[2] += Z.c[i] * _samples[i];
        //    }

        //    var scale = (float) (SampledLambdaEnd - SampledLambdaStart) / SpectrumSamples;
        //    xyz[0] *= scale;
        //    xyz[1] *= scale;
        //    xyz[2] *= scale;
        //    return xyz;
        //}
    }
}