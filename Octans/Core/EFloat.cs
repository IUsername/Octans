using System;
using System.Diagnostics.Contracts;
using static System.MathF;
using static System.Single;

namespace Octans
{
    public class EFloat : IEquatable<EFloat>
    {
        public EFloat(float v, float err = 0f)
        {
            V = v;
            if (err == 0f)
            {
                Low = High = v;
            }
            else
            {
                Low = BitDecrement(v - err);
                High = BitIncrement(v + err);
            }
        }

        public EFloat(in EFloat ef)
        {
            V = ef.V;
            Low = ef.Low;
            High = ef.High;
        }

        private EFloat(float v, float high, float low)
        {
            V = v;
            Low = low;
            High = high;
        }

        public float V { get; }

        private float High { get; }

        private float Low { get; }

        public bool Equals(EFloat other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || V.Equals(other.V);
        }

        public float UpperBound() => High;

        public float LowerBound() => Low;

        [Pure]
        public EFloat Add(in EFloat ef) =>
            new EFloat(V + ef.V,
                       BitIncrement(High + ef.High),
                       BitDecrement(Low + ef.Low));

        [Pure]
        public EFloat Subtract(in EFloat ef) =>
            new EFloat(V - ef.V,
                       BitIncrement(High - ef.Low),
                       BitDecrement(Low - ef.High));

        [Pure]
        public EFloat Multiply(in EFloat ef)
        {
            var newV = V * ef.V;
            var prod = new[]
            {
                Low * ef.Low,
                High * ef.Low,
                Low * ef.High,
                High * ef.High
            };
            var newLow = BitDecrement(Min(Min(prod[0], prod[1]), Min(prod[2], prod[3])));
            var newHigh = BitIncrement(Max(Max(prod[0], prod[1]), Max(prod[2], prod[3])));
            return new EFloat(newV, newHigh, newLow);
        }

        [Pure]
        public EFloat Divide(in EFloat ef)
        {
            var newV = V / ef.V;
            if (ef.Low < 0 && ef.High > 0)
            {
                return new EFloat(newV, PositiveInfinity, NegativeInfinity);
            }

            var prod = new[]
            {
                Low / ef.Low,
                High / ef.Low,
                Low / ef.High,
                High / ef.High
            };
            var newLow = BitDecrement(Min(Min(prod[0], prod[1]), Min(prod[2], prod[3])));
            var newHigh = BitIncrement(Max(Max(prod[0], prod[1]), Max(prod[2], prod[3])));
            return new EFloat(newV, newHigh, newLow);
        }

        [Pure]
        public EFloat Negate() => new EFloat(-V, -High, -Low);

        [Pure]
        public float GetAbsoluteError() => High - Low;

        [Pure]
        public static EFloat Sqrt(in EFloat ef) =>
            new EFloat(System.MathF.Sqrt(ef.V),
                       BitIncrement(System.MathF.Sqrt(ef.High)),
                       BitDecrement(System.MathF.Sqrt(ef.Low)));

        [Pure]
        public static EFloat Abs(in EFloat ef)
        {
            if (ef.Low > 0f)
            {
                return ef;
            }

            if (ef.High <= 0f)
            {
                return new EFloat(-ef.V, -ef.High, -ef.Low);
            }

            return new EFloat(System.MathF.Abs(ef.V),
                              Max(-ef.Low, ef.High),
                              0f);
        }

        public static bool Quadratic(EFloat A, EFloat B, EFloat C, out EFloat t0, out EFloat t1)
        {
            var discriminant = (double) B * (double) B - 4.0 * (double) A * (double) C;
            if (discriminant < 0.0)
            {
                t0 = new EFloat(0);
                t1 = new EFloat(0);
                return false;
            }

            var rootDiscriminant = Math.Sqrt(discriminant);
            var floatRootDiscriminant = new EFloat((float) rootDiscriminant, (float) (Epsilon * rootDiscriminant));

            EFloat q;
            if ((float) B < 0f)
            {
                q = -0.5f * (B - floatRootDiscriminant);
            }
            else
            {
                q = -0.5f * (B + floatRootDiscriminant);
            }

            t0 = q / A;
            t1 = C / q;
            if ((float) t0 > (float) t1)
            {
                (t0, t1) = (t1, t0);
            }

            return true;
        }

        public static EFloat operator +(EFloat left, EFloat right) => left.Add(in right);

        public static EFloat operator +(float left, EFloat right) => (EFloat) left + right;

        public static EFloat operator -(EFloat left, EFloat right) => left.Subtract(in right);

        public static EFloat operator -(float left, EFloat right) => (EFloat) left - right;

        public static EFloat operator *(EFloat left, EFloat right) => left.Multiply(in right);

        public static EFloat operator *(float left, EFloat right) => (EFloat) left * right;

        public static EFloat operator /(EFloat left, EFloat right) => left.Divide(in right);

        public static EFloat operator /(float left, EFloat right) => (EFloat) left / right;

        public static EFloat operator -(EFloat ef) => ef.Negate();

        public static bool operator ==(EFloat left, EFloat right) => Equals(left, right);

        public static bool operator !=(EFloat left, EFloat right) => !Equals(left, right);

        public static explicit operator float(EFloat ef) => ef.V;

        public static explicit operator double(EFloat ef) => ef.V;

        public static explicit operator EFloat(float v) => new EFloat(v);

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

            return obj.GetType() == GetType() && Equals((EFloat) obj);
        }

        public override int GetHashCode() => V.GetHashCode();
    }
}