using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using static System.MathF;
using static System.Numerics.Vector;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StaticMemberInGenericType

namespace Octans
{
    public enum SpectrumType
    {
        Reflectance,
        Illuminant
    }


    public struct Spectrum : IEquatable<Spectrum>
    {
        private static readonly int SimdLength;
        private static readonly int RemainderIndex;

        public static readonly int Samples;
        public static readonly int SampledLambdaEnd;
        public static readonly int SampledLambdaStart;
        public static readonly Spectrum Zero;
        public static readonly Spectrum One;

        internal static readonly Spectrum X;
        internal static readonly Spectrum Y;
        internal static readonly Spectrum Z;

        private static readonly Spectrum RGBRefl2SpectWhite;
        private static readonly Spectrum RGBRefl2SpectCyan;
        private static readonly Spectrum RGBRefl2SpectMagenta;
        private static readonly Spectrum RGBRefl2SpectYellow;
        private static readonly Spectrum RGBRefl2SpectRed;
        private static readonly Spectrum RGBRefl2SpectGreen;
        private static readonly Spectrum RGBRefl2SpectBlue;
        private static readonly Spectrum RGBIllum2SpectWhite;
        private static readonly Spectrum RGBIllum2SpectCyan;
        private static readonly Spectrum RGBIllum2SpectMagenta;
        private static readonly Spectrum RGBIllum2SpectYellow;
        private static readonly Spectrum RGBIllum2SpectRed;
        private static readonly Spectrum RGBIllum2SpectGreen;
        private static readonly Spectrum RGBIllum2SpectBlue;

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

            RGBRefl2SpectWhite = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectWhite));
            RGBRefl2SpectCyan = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectCyan));
            RGBRefl2SpectMagenta = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectMagenta));
            RGBRefl2SpectYellow = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectYellow));
            RGBRefl2SpectRed = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectRed));
            RGBRefl2SpectGreen = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectGreen));
            RGBRefl2SpectBlue = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBRefl2SpectBlue));
            RGBIllum2SpectWhite = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectWhite));
            RGBIllum2SpectCyan = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectCyan));
            RGBIllum2SpectMagenta = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectMagenta));
            RGBIllum2SpectYellow = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectYellow));
            RGBIllum2SpectRed = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectRed));
            RGBIllum2SpectGreen = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectGreen));
            RGBIllum2SpectBlue = new Spectrum(spectral.GetRGBSpectrum(CIE_Data.RGBIllum2SpectBlue));

            Zero = new Spectrum(0f);
            One = new Spectrum(1f);
        }

        public Spectrum(float v)
        {
            Channels = new float[Samples];
            Array.Fill(Channels, v);
            //for (var i = 0; i < Samples; i++)
            //{
            //    Channels[i] = v;
            //}
        }


        //public Spectrum()
        //{
        //    _c = new float[Samples];
        //}

        internal Spectrum(float[] c)
        {
            Channels = c;
        }

        internal Spectrum Initialize(float[] c)
        {
            Debug.Assert(c.Length == Samples);
            Channels = c;
            return this;
        }

        //internal ref readonly float[] C => ref _c;

        internal float[] Channels { get; private set; }

        [Pure]
        public float this[int index] => Channels[index];

        [Pure]
        public bool Equals(Spectrum other)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(other.Channels, i);
                if (!EqualsAll(a, b))
                {
                    return false;
                }
            }

            for (; i < Samples; i++)
            {
                if (Channels[i].Equals(other.Channels[i]))
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
                var vector = new Vector<float>(Channels, i);
                if (!EqualsAll(vector, Vector<float>.Zero))
                {
                    return false;
                }
            }

            for (; i < Samples; i++)
            {
                if (Channels[i] != 0f)
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
                if (float.IsNaN(Channels[i]))
                {
                    return true;
                }
            }

            return false;
        }

        [Pure]
        public Spectrum Add(in Spectrum other)
        {
            //if (Samples == 3)
            //{
            //    //_c[0] += other._c[0];
            //    //_c[1] += other._c[1];
            //    //_c[2] += other._c[2];
            //    //return this;

            //    return new Spectrum(new[]
            //    {
            //        Channels[0] + other.Channels[0],
            //        Channels[1] + other.Channels[1],
            //        Channels[2] + other.Channels[2]
            //    });
            //}

            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Add(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] + other.Channels[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Add(in float scalar)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(scalar);
                System.Numerics.Vector.Add(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] + scalar;
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
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Subtract(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] - other.Channels[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Subtract(in float scalar)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(scalar);
                System.Numerics.Vector.Subtract(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] - scalar;
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
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Multiply(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] * other.Channels[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Multiply(float scalar)
        {
            //if (Samples == 3)
            //{
            //    //_c[0] *= scalar;
            //    //_c[1] *= scalar;
            //    //_c[2] *= scalar;
            //    //return this;
            //    return new Spectrum(new[] {Channels[0] * scalar, Channels[1] * scalar, Channels[2] * scalar});
            //}

            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                System.Numerics.Vector.Multiply(a, scalar).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] * scalar;
            }

            //var results = new float[Samples];
            //for (var i = 0; i < Samples; i++)
            //{
            //    results[i] = Channels[i] * scalar;
            //}

            return new Spectrum(results);
        }

        [Pure]
        public static Spectrum FusedMultiply(Spectrum a, Spectrum b, float c)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var aC = new Vector<float>(a.Channels, i);
                var bC = new Vector<float>(b.Channels, i);

                System.Numerics.Vector.Multiply(aC, System.Numerics.Vector.Multiply(bC, c))
                      .CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = a.Channels[i] * b.Channels[i] * c;
            }

            return new Spectrum(results);
        }

        //public void Scale(float scalar)
        //{
        //    int i;
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(Channels, i);
        //        System.Numerics.Vector.Multiply(a, scalar).CopyTo(Channels, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        Channels[i] *= scalar;
        //    }
        //}

        //public void Scale(Spectrum s)
        //{
        //    int i;
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(Channels, i);
        //        var b = new Vector<float>(s.Channels, i);
        //        System.Numerics.Vector.Multiply(a, b).CopyTo(Channels, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        Channels[i] *= s.Channels[i];
        //    }
        //}

        [Pure]
        public Spectrum Divide(in Spectrum other)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(Channels, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Divide(a, b).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] / other.Channels[i];
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
                var a = new Vector<float>(Channels, i);
                System.Numerics.Vector.Multiply(a, inv).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = Channels[i] * inv;
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
                var a = new Vector<float>(Channels, i);
                SquareRoot(a).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = System.MathF.Sqrt(Channels[i]);
            }

            return new Spectrum(results);
        }

        [Pure]
        public Spectrum Pow(float y)
        {
            var s = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                s[i] = System.MathF.Pow(Channels[i], y);
            }

            return new Spectrum(s);
        }

        [Pure]
        public Spectrum Exp()
        {
            var s = new float[Samples];
            for (var i = 0; i < Samples; i++)
            {
                s[i] = System.MathF.Exp(Channels[i]);
            }

            return new Spectrum(s);
        }

        [Pure]
        public Spectrum Clamp(float low = 0f, float high = float.PositiveInfinity) => Clamp(this, low, high);

        [Pure]
        public static Spectrum Negate(in Spectrum spectrum)
        {
            int i;
            var results = new float[Samples];
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(spectrum.Channels, i);
                System.Numerics.Vector.Negate(a).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = -spectrum.Channels[i];
            }

            return new Spectrum(results);
        }

        [Pure]
        public static Spectrum Lerp(in Spectrum s0, in Spectrum s1, float t)
        {
            if (t == 0f) return s0;
            if (t == 1f) return s1;

            //int i;
            //var oneMinusT = 1f - t;
            //var results = new float[Samples];
            //for (i = 0; i < RemainderIndex; i += SimdLength)
            //{
            //    var a = new Vector<float>(s0.Channels, i);
            //    var b = new Vector<float>(s1.Channels, i);
            //    System.Numerics.Vector.Add(
            //              System.Numerics.Vector.Multiply(a, oneMinusT),
            //              System.Numerics.Vector.Multiply(b, t))
            //          .CopyTo(results, i);
            //}

            var results = new float[Samples];
            for (var i=0; i < Samples; i++)
            {
                results[i] = MathF.Lerp(s0.Channels[i], s1.Channels[i], t);
            }

            return new Spectrum(results);
        }

        [Pure]
        public static Spectrum Clamp(in Spectrum spectrum, float low = 0f, float high = float.PositiveInfinity)
        {
            int i;
            var results = new float[Samples];
            var l = new Vector<float>(low);
            var h = new Vector<float>(high);
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(spectrum.Channels, i);
                Min(Max(a, l), h).CopyTo(results, i);
            }

            for (; i < Samples; i++)
            {
                results[i] = MathF.Clamp(low, high, spectrum.Channels[i]);
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
                var a = new Vector<float>(spectrum.Channels, i);
                vMax = Max(a, vMax);
            }

            var max = float.MinValue;
            for (var j = 0; j < SimdLength; j++)
            {
                max = Max(max, vMax[j]);
            }

            for (; i < Samples; i++)
            {
                max = Max(spectrum.Channels[i], max);
            }

            return max;
        }

        [Pure]
        public float[] ToXYZ()
        {
            var xyz = new float[3];
            for (var i = 0; i < Samples; i++)
            {
                xyz[0] += X.Channels[i] * Channels[i];
                xyz[1] += Y.Channels[i] * Channels[i];
                xyz[2] += Z.Channels[i] * Channels[i];
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
                yy += Y.Channels[i] * Channels[i];
            }

            return yy * (SampledLambdaEnd - SampledLambdaStart) / (CIE_Data.CIE_Y_integral * Samples);
        }

        [Pure]
        public static float[] XYZToRGB(in float[] xyz)
        {
            var rgb = new float[3];
            rgb[0] = 3.240479f * xyz[0] - 1.537150f * xyz[1] - 0.498535f * xyz[2];
            rgb[1] = -0.969256f * xyz[0] + 1.875991f * xyz[1] + 0.041556f * xyz[2];
            rgb[2] = 0.055648f * xyz[0] - 0.204043f * xyz[1] + 1.057311f * xyz[2];
            return rgb;
        }


        [Pure]
        public static float[] XYZToRGB(in float x, in float y, in float z)
        {
            var rgb = new float[3];
            rgb[0] = 3.240479f * x - 1.537150f * y - 0.498535f * z;
            rgb[1] = -0.969256f * x + 1.875991f * y + 0.041556f * z;
            rgb[2] = 0.055648f * x - 0.204043f * y + 1.057311f * z;
            return rgb;
        }

        [Pure]
        public static void XYZToRGB(in float x, in float y, in float z, in Span<float> rgb)
        {
            rgb[0] = 3.240479f * x - 1.537150f * y - 0.498535f * z;
            rgb[1] = -0.969256f * x + 1.875991f * y + 0.041556f * z;
            rgb[2] = 0.055648f * x - 0.204043f * y + 1.057311f * z;
        }

        [Pure]
        public static float[] RGBToXYZ(in float[] rgb)
        {
            var xyz = new float[3];
            xyz[0] = 0.412453f * rgb[0] + 0.357580f * rgb[1] + 0.180423f * rgb[2];
            xyz[1] = 0.212671f * rgb[0] + 0.715160f * rgb[1] + 0.072169f * rgb[2];
            xyz[2] = 0.019334f * rgb[0] + 0.119193f * rgb[1] + 0.950227f * rgb[2];
            return xyz;
        }

        [Pure]
        public static float[] RGBToXYZ(in float r, in float g, in float b)
        {
            var xyz = new float[3];
            xyz[0] = 0.412453f * r + 0.357580f * g + 0.180423f * b;
            xyz[1] = 0.212671f * r + 0.715160f * g + 0.072169f * b;
            xyz[2] = 0.019334f * r + 0.119193f * g + 0.950227f * b;
            return xyz;
        }

        [Pure]
        public static Spectrum FromRGB(in float[] rgb, SpectrumType type)
        {
            var r = Zero;
            if (type == SpectrumType.Reflectance)
            {
                if (rgb[0] <= rgb[1] && rgb[0] <= rgb[2])
                {
                    r += rgb[0] * RGBRefl2SpectWhite;
                    if (rgb[1] <= rgb[2])
                    {
                        r += (rgb[1] - rgb[0]) * RGBRefl2SpectCyan;
                        r += (rgb[2] - rgb[1]) * RGBRefl2SpectBlue;
                    }
                    else
                    {
                        r += (rgb[2] - rgb[0]) * RGBRefl2SpectCyan;
                        r += (rgb[1] - rgb[2]) * RGBRefl2SpectGreen;
                    }
                }
                else if (rgb[1] <= rgb[0] && rgb[1] <= rgb[2])
                {
                    r += rgb[1] * RGBRefl2SpectWhite;
                    if (rgb[0] <= rgb[2])
                    {
                        r += (rgb[0] - rgb[1]) * RGBRefl2SpectMagenta;
                        r += (rgb[2] - rgb[0]) * RGBRefl2SpectBlue;
                    }
                    else
                    {
                        r += (rgb[2] - rgb[1]) * RGBRefl2SpectMagenta;
                        r += (rgb[0] - rgb[2]) * RGBRefl2SpectRed;
                    }
                }
                else
                {
                    r += rgb[2] * RGBRefl2SpectWhite;
                    if (rgb[0] <= rgb[1])
                    {
                        r += (rgb[0] - rgb[2]) * RGBRefl2SpectYellow;
                        r += (rgb[1] - rgb[0]) * RGBRefl2SpectGreen;
                    }
                    else
                    {
                        r += (rgb[1] - rgb[2]) * RGBRefl2SpectYellow;
                        r += (rgb[0] - rgb[1]) * RGBRefl2SpectRed;
                    }
                }

                r *= 0.94f;
            }
            else
            {
                if (rgb[0] <= rgb[1] && rgb[0] <= rgb[2])
                {
                    r += rgb[0] * RGBIllum2SpectWhite;
                    if (rgb[1] <= rgb[2])
                    {
                        r += (rgb[1] - rgb[0]) * RGBIllum2SpectCyan;
                        r += (rgb[2] - rgb[1]) * RGBIllum2SpectBlue;
                    }
                    else
                    {
                        r += (rgb[2] - rgb[0]) * RGBIllum2SpectCyan;
                        r += (rgb[1] - rgb[2]) * RGBIllum2SpectGreen;
                    }
                }
                else if (rgb[1] <= rgb[0] && rgb[1] <= rgb[2])
                {
                    r += rgb[1] * RGBIllum2SpectWhite;
                    if (rgb[0] <= rgb[2])
                    {
                        r += (rgb[0] - rgb[1]) * RGBIllum2SpectMagenta;
                        r += (rgb[2] - rgb[0]) * RGBIllum2SpectBlue;
                    }
                    else
                    {
                        r += (rgb[2] - rgb[1]) * RGBIllum2SpectMagenta;
                        r += (rgb[0] - rgb[2]) * RGBIllum2SpectRed;
                    }
                }
                else
                {
                    r += rgb[2] * RGBIllum2SpectWhite;
                    if (rgb[0] <= rgb[1])
                    {
                        r += (rgb[0] - rgb[2]) * RGBIllum2SpectYellow;
                        r += (rgb[1] - rgb[0]) * RGBIllum2SpectGreen;
                    }
                    else
                    {
                        r += (rgb[1] - rgb[2]) * RGBIllum2SpectYellow;
                        r += (rgb[0] - rgb[1]) * RGBIllum2SpectRed;
                    }
                }
            }

            return r.Clamp();
        }

        [Pure]
        public static Spectrum FromBlackbodyT(float T)
        {
            var spectral = new SpectralInformation();
            return new Spectrum(spectral.FromBlackbodyT(T));
        }

        [Pure]
        public static Spectrum operator +(Spectrum left, Spectrum right) => left.Add(in right);

        [Pure]
        public static Spectrum operator +(Spectrum left, float right) => left.Add(in right);

        [Pure]
        public static Spectrum operator +(float left, Spectrum right) => right.Add(in left);

        [Pure]
        public static Spectrum operator -(Spectrum left, Spectrum right) => left.Subtract(in right);

        [Pure]
        public static Spectrum operator -(Spectrum left, float right) => left.Subtract(in right);

        [Pure]
        public static Spectrum operator -(float left, Spectrum right) => -right.Add(in left);

        [Pure]
        public static Spectrum operator *(Spectrum left, Spectrum right) => left.Multiply(in right);

        [Pure]
        public static Spectrum operator *(Spectrum left, float right) => left.Multiply(right);

        [Pure]
        public static Spectrum operator *(float left, Spectrum right) => right.Multiply(left);

        [Pure]
        public static Spectrum operator /(Spectrum left, Spectrum right) => left.Divide(in right);

        [Pure]
        public static Spectrum operator /(Spectrum left, float right) => left.Divide(right);

        [Pure]
        public static Spectrum operator ^(Spectrum s, float y) => s.Pow(y);

        [Pure]
        public static Spectrum operator -(Spectrum s) => Negate(in s);

        [Pure]
        public static bool Equals(Spectrum x, Spectrum y) => x.Equals(y);

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

            //if (ReferenceEquals(this, obj))
            //{
            //    return true;
            //}

            return obj.GetType() == GetType() && Equals((Spectrum) obj);
        }

        [Pure]
        public override int GetHashCode() => Channels.GetHashCode();
    }
}