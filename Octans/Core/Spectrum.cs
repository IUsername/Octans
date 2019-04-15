using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable StaticMemberInGenericType

namespace Octans
{
    public enum SpectralData
    {
        RGBToSpectralLambda
    }


    public class Spectrum : IEquatable<Spectrum>
    {
        private static readonly int SimdLength;
        private static readonly int RemainderIndex;

        public static readonly int Samples;
        public static readonly int SampledLambdaEnd;
        public static readonly int SampledLambdaStart;
        public static readonly Spectrum Black = new Spectrum();

        private static readonly Spectrum X;
        private static readonly Spectrum Y;
        private static readonly Spectrum Z;
      
        private readonly float[] _c;

        static Spectrum()
        {
            var spectral = new SpectralInformation();
            Samples = spectral.Samples;
            SampledLambdaStart = spectral.SampledLambdaStart;
            SampledLambdaEnd = spectral.SampledLambdaEnd;

            SimdLength = Vector<float>.Count;
            RemainderIndex = Samples - Samples % SimdLength;

            var (x, y, z) = spectral.GetXYZ();
            X = new Spectrum(x);
            Y = new Spectrum(y);
            Z = new Spectrum(z);
        }

        public Spectrum(float v)
        {
            _c = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                _c[i] = v;
            }
        }

        public Spectrum()
        {
            _c = new float[Samples];
        }

        private Spectrum(float[] c)
        {
            _c = c;
        }

        protected ref readonly float[] C => ref _c;

        [Pure]
        public float this[int index] => _c[index];

        [Pure]
        public bool Equals(Spectrum other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                if (!System.Numerics.Vector.EqualsAll(a, b))
                {
                    return false;
                }
            }

            for (; i < Samples; i++)
            {
                if (_c[i].Equals(other._c[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [Pure]
        public bool IsBlack()
        {
            //return Equals(Black);
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var vector = new Vector<float>(_c, i);
                if (!System.Numerics.Vector.EqualsAll(vector, Vector<float>.Zero))
                {
                    return false;
                }
            }

            for (; i < Samples; i++)
            {
                if (_c[i] != 0f)
                {
                    return false;
                }
            }

            return true;
        }

        [Pure]
        public bool HasNaN()
        {
            for (var i = 0; i < Samples; i++)
            {
                if (float.IsNaN(_c[i]))
                {
                    return true;
                }
            }

            return false;
        }

        [Pure]
        public Spectrum Add(in Spectrum other)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Add(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] + other._c[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Subtract(in Spectrum other)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Subtract(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] - other._c[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Multiply(in Spectrum other)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Multiply(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] * other._c[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Multiply(float scalar)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                System.Numerics.Vector.Multiply(a, scalar).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] * scalar;
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Divide(in Spectrum other)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Divide(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] / other._c[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Divide(float scalar)
        {
            int i;
            var inv = 1f / scalar;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                System.Numerics.Vector.Multiply(a, inv).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = _c[i] * inv;
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Sqrt()
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                System.Numerics.Vector.SquareRoot(a).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = MathF.Sqrt(_c[i]);
            }

            return new Spectrum(results);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public Spectrum Pow(float y)
        {
            var s = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                s[i] = MathF.Pow(_c[i], y);
            }

            return new Spectrum(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public Spectrum Exp()
        {
            var s = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                s[i] = MathF.Exp(_c[i]);
            }

            return new Spectrum(s);
        }

        [Pure]
        public static Spectrum Negate(in Spectrum spectrum)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(spectrum._c, i);
                System.Numerics.Vector.Negate(a).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = -spectrum._c[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public static Spectrum Lerp(in Spectrum s0, in Spectrum s1, float t)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(s0._c, i);
                var b = new Vector<float>(s1._c, i);
                System.Numerics.Vector.Add(
                          System.Numerics.Vector.Multiply(a, 1f - t),
                          System.Numerics.Vector.Multiply(b, t))
                      .CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = MathFunction.Lerp(s0._c[i], s1._c[i], t);
            }

            return new Spectrum(results);
        }

        [Pure]
        public static Spectrum Clamp(in Spectrum spectrum, float low, float high = float.PositiveInfinity)
        {
            int i;
            var results = new float[Samples];
            var l = new Vector<float>(low);
            var h = new Vector<float>(high);
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(spectrum._c, i);
                System.Numerics.Vector.Min(System.Numerics.Vector.Max(a, l), h).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = MathFunction.ClampF(low, high, spectrum._c[i]);
            }

            return new Spectrum(results);
        }

        [Pure]
        public static float MaxComponent(in Spectrum spectrum)
        {
            int i;
            var vMax = new Vector<float>(float.MinValue);
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(spectrum._c, i);
                vMax = System.Numerics.Vector.Max(a, vMax);
            }

            var max = float.MinValue;
            for (var j = 0; j < SimdLength; j++)
            {
                max = MathF.Max(max, vMax[j]);
            }

            for (; i < Samples; i++)
            {
                max = MathF.Max(spectrum._c[i], max);
            }

            return max;
        }

        [Pure]
        public float[] ToXYZ()
        {
            var xyz = new float[3];
            for (var i = 0; i < Samples; i++)
            {
                xyz[0] += X.C[i] * C[i];
                xyz[1] += Y.C[i] * C[i];
                xyz[2] += Z.C[i] * C[i];
            }

            var scale = (SampledLambdaEnd - SampledLambdaStart) / (CIE_Data.CIE_Y_integral * Samples);
            xyz[0] *= scale;
            xyz[1] *= scale;
            xyz[2] *= scale;
            return xyz;
        }

        [Pure]
        public float YComponent()
        {
            var yy = 0f;
            for (var i = 0; i < Samples; i++)
            {
                yy += Y.C[i] * C[i];
            }

            return yy * (SampledLambdaEnd - SampledLambdaStart) / (CIE_Data.CIE_Y_integral * Samples);
        }

        [Pure]
        public static Spectrum operator +(Spectrum left, Spectrum right) => left.Add(in right);

        [Pure]
        public static Spectrum operator -(Spectrum left, Spectrum right) => left.Subtract(in right);

        [Pure]
        public static Spectrum operator *(Spectrum left, Spectrum right) => left.Multiply(in right);

        [Pure]
        public static Spectrum operator *(Spectrum left, float right) => left.Multiply(right);

        [Pure]
        public static Spectrum operator /(Spectrum left, Spectrum right) => left.Divide(in right);

        [Pure]
        public static Spectrum operator /(Spectrum left, float right) => left.Divide(right);

        [Pure]
        public static Spectrum operator ^(Spectrum s, float y) => s.Pow(y);

        [Pure]
        public static Spectrum operator -(Spectrum s) => Negate(in s);

        [Pure]
        public static bool Equals(Spectrum x, Spectrum y) => !(x is null) && x.Equals(y);

        [Pure]
        public static bool operator ==(Spectrum left, Spectrum right) => Equals(left, right);

        [Pure]
        public static bool operator !=(Spectrum left, Spectrum right) => !Equals(left, right);

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

            return obj.GetType() == GetType() && Equals((Spectrum) obj);
        }

        [Pure]
        public override int GetHashCode() => _c.GetHashCode();
    }
}