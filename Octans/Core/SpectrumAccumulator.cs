using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace Octans
{
    public class SpectrumAccumulator// : IEquatable<SpectrumAccumulator>
    {
        private static readonly int SimdLength;
        private static readonly int RemainderIndex;

        public static readonly int Samples;

        private readonly float[] _c;

        static SpectrumAccumulator()
        {
            Samples = Spectrum.Samples;

            SimdLength = Vector<float>.Count;
            RemainderIndex = Samples - Samples % SimdLength;
    
            //X = new SpectrumAccumulator(Spectrum.X);
            //Y = new SpectrumAccumulator(Spectrum.Y);
            //Z = new SpectrumAccumulator(Spectrum.Z);
        }

        //public SpectrumAccumulator(float v)
        //{
        //    _c = new float[Samples];
        //    for (var i = 0; i < Samples; i++)
        //    {
        //        _c[i] = v;
        //    }
        //}

        public SpectrumAccumulator()
        {
            _c = new float[Samples];
        }

        //private SpectrumAccumulator(float[] c)
        //{
        //    _c = c;
        //}

        //private SpectrumAccumulator(Spectrum s)
        //{
        //    _c = s.Components;
        //}

        public SpectrumAccumulator Zero()
        {
            Array.Fill(_c, 0f);
            return this;
        }

        public SpectrumAccumulator FromSpectrum(Spectrum s)
        {
            Array.Copy(s.Channels, _c, Samples);
            return this;
        }

        [Pure]
        public Spectrum ToSpectrum()
        {
            return new Spectrum(_c);
        }

        [Pure]
        public Spectrum ToSpectrum(IObjectArena arena)
        {
            var c = new float[_c.Length];
            Array.Copy(_c, c, _c.Length);
            return arena.Create<Spectrum>().Initialize(c);
        }

        protected ref readonly float[] C => ref _c;

        [Pure]
        public float this[int index] => _c[index];

        //[Pure]
        //public bool Equals(SpectrumAccumulator other)
        //{
        //    if (other is null)
        //    {
        //        return false;
        //    }

        //    if (ReferenceEquals(this, other))
        //    {
        //        return true;
        //    }

        //    int i;
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        var b = new Vector<float>(other._c, i);
        //        if (!System.Numerics.Vector.EqualsAll(a, b))
        //        {
        //            return false;
        //        }
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        if (_c[i].Equals(other._c[i]))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

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
       
        public void Contribute(in SpectrumAccumulator other)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Add(a, b).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += other._c[i];
            }
        }

        public void Contribute(in Spectrum other)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Add(a, b).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += other.Channels[i];
            }
        }

        public void Contribute(in Spectrum other, float scale)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Add(a, System.Numerics.Vector.Multiply(b, scale)).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += other.Channels[i] * scale;
            }
        }

        public void Contribute(in SpectrumAccumulator other, float scale)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Add(a, System.Numerics.Vector.Multiply(b, scale)).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += other._c[i] * scale;
            }
        }

        public void Contribute(in Spectrum other, in Spectrum scale)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other.Channels, i);
                var c = new Vector<float>(scale.Channels, i);
                System.Numerics.Vector.Add(a, System.Numerics.Vector.Multiply(b, c)).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += other.Channels[i] * scale.Channels[i];
            }
        }

        public void Contribute(in float scalar)
        {
            int i;
            var b = new Vector<float>(scalar);
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                System.Numerics.Vector.Add(a, b).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] += scalar;
            }

        }

        //[Pure]
        //public SpectrumAccumulator Subtract(in SpectrumAccumulator other)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        var b = new Vector<float>(other._c, i);
        //        System.Numerics.Vector.Subtract(a, b).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = _c[i] - other._c[i];
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public SpectrumAccumulator Subtract(in float scalar)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    var b = new Vector<float>(scalar);
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        System.Numerics.Vector.Subtract(a, b).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = _c[i] - scalar;
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        public void Scale(in SpectrumAccumulator other)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other._c, i);
                System.Numerics.Vector.Multiply(a, b).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] *= other._c[i];
            }
        }

        public void Scale(in Spectrum other)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                var b = new Vector<float>(other.Channels, i);
                System.Numerics.Vector.Multiply(a, b).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] *= other.Channels[i];
            }
        }

        public void Scale(float scalar)
        {
            int i;
            for (i = 0; i < RemainderIndex; i += SimdLength)
            {
                var a = new Vector<float>(_c, i);
                System.Numerics.Vector.Multiply(a, scalar).CopyTo(_c, i);
            }

            for (; i < Samples; i++)
            {
                _c[i] *= scalar;
            }
        }

        //[Pure]
        //public SpectrumAccumulator Divide(in SpectrumAccumulator other)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        var b = new Vector<float>(other._c, i);
        //        System.Numerics.Vector.Divide(a, b).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = _c[i] / other._c[i];
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public SpectrumAccumulator Divide(float scalar)
        //{
        //    int i;
        //    var inv = 1f / scalar;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        System.Numerics.Vector.Multiply(a, inv).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = _c[i] * inv;
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public SpectrumAccumulator Sqrt()
        //{
        //    int i;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(_c, i);
        //        System.Numerics.Vector.SquareRoot(a).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = System.MathF.Sqrt(_c[i]);
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public SpectrumAccumulator Pow(float y)
        //{
        //    var s = new float[Samples];
        //    for (var i = 0; i < Samples; i++)
        //    {
        //        s[i] = System.MathF.Pow(_c[i], y);
        //    }

        //    return new SpectrumAccumulator(s);
        //}

        //[Pure]
        //public SpectrumAccumulator Exp()
        //{
        //    var s = new float[Samples];
        //    for (var i = 0; i < Samples; i++)
        //    {
        //        s[i] = System.MathF.Exp(_c[i]);
        //    }

        //    return new SpectrumAccumulator(s);
        //}

        //[Pure]
        //public SpectrumAccumulator Clamp(float low = 0f, float high = float.PositiveInfinity) => Clamp(this, low, high);

        //[Pure]
        //public static SpectrumAccumulator Negate(in SpectrumAccumulator spectrum)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(spectrum._c, i);
        //        System.Numerics.Vector.Negate(a).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = -spectrum._c[i];
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public static SpectrumAccumulator Lerp(in SpectrumAccumulator s0, in SpectrumAccumulator s1, float t)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(s0._c, i);
        //        var b = new Vector<float>(s1._c, i);
        //        System.Numerics.Vector.Add(
        //                  System.Numerics.Vector.Multiply(a, 1f - t),
        //                  System.Numerics.Vector.Multiply(b, t))
        //              .CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = MathF.Lerp(s0._c[i], s1._c[i], t);
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public static SpectrumAccumulator Clamp(in SpectrumAccumulator spectrum, float low = 0f, float high = float.PositiveInfinity)
        //{
        //    int i;
        //    var results = new float[Samples];
        //    var l = new Vector<float>(low);
        //    var h = new Vector<float>(high);
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(spectrum._c, i);
        //        System.Numerics.Vector.Min(System.Numerics.Vector.Max(a, l), h).CopyTo(results, i);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        results[i] = MathF.Clamp(low, high, spectrum._c[i]);
        //    }

        //    return new SpectrumAccumulator(results);
        //}

        //[Pure]
        //public static float MaxComponent(in SpectrumAccumulator spectrum)
        //{
        //    int i;
        //    var vMax = new Vector<float>(float.MinValue);
        //    for (i = 0; i < RemainderIndex; i += SimdLength)
        //    {
        //        var a = new Vector<float>(spectrum._c, i);
        //        vMax = System.Numerics.Vector.Max(a, vMax);
        //    }

        //    var max = float.MinValue;
        //    for (var j = 0; j < SimdLength; j++)
        //    {
        //        max = System.MathF.Max(max, vMax[j]);
        //    }

        //    for (; i < Samples; i++)
        //    {
        //        max = System.MathF.Max(spectrum._c[i], max);
        //    }

        //    return max;
        //}

        [Pure]
        public float[] ToXYZ()
        {
            var xyz = new float[3];
            for (var i = 0; i < Samples; i++)
            {
                xyz[0] += Spectrum.X.Channels[i] * C[i];
                xyz[1] += Spectrum.Y.Channels[i] * C[i];
                xyz[2] += Spectrum.Z.Channels[i] * C[i];
            }

            var scale = (Spectrum.SampledLambdaEnd - Spectrum.SampledLambdaStart) / (CIE_Data.CIE_Y_integral * Samples);
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
                yy += Spectrum.Y.Channels[i] * C[i];
            }

            return yy * (Spectrum.SampledLambdaEnd - Spectrum.SampledLambdaStart) / (CIE_Data.CIE_Y_integral * Samples);
        }
    }
}