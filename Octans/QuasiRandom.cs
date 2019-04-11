using System;
using System.Runtime.CompilerServices;

namespace Octans
{
    public static class QuasiRandom
    {
        /// <summary>
        ///     Returns a quasi-random float in [0..1] based on index and base.
        /// </summary>
        /// <param name="n">Index in sequence.</param>
        /// <param name="b">Number base.</param>
        /// <returns></returns>
        //private static float VanDerCorput(long n, int b)
        //{
        //    long p = 0, q = 1;
        //    while (n != 0)
        //    {
        //        p = p * b + n % b;
        //        q *= b;
        //        n /= b;
        //    }

        //    var numerator = p;
        //    var denominator = q;
        //    while (p != 0)
        //    {
        //        n = p;
        //        p = q % p;
        //        q = n;
        //    }

        //    numerator /= q;
        //    denominator /= q;
        //    return numerator / (float) denominator;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static float VanDerCorput(long n, int b)
        //{
        //    float p = 0, q = 1;
        //    while (n > 0)
        //    {
        //        p += (n % b) / (q *= b);
        //        n /= b;
        //    }

        //    return p;
        //}

        /// <summary>
        ///     Returns a quasi-random float in [0..1) based on index and base.
        /// </summary>
        /// <param name="n">Index in sequence.</param>
        /// <param name="b">Number base.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float VanDerCorput2(long n, int b)
        {
            switch (b)
            {
                case 2:
                   // const float inv2 = 1f / 2f;
                    //  var rib = RadicalInverseOpt((ulong)n, (uint)b, inv2);
                    return RadicalInverseBase2((ulong)n);
                case 3:
                    const float inv3 = 1f / 3f;
                    return RadicalInverseOpt((ulong)n, (uint)b, inv3);
                case 5:
                    const float inv5 = 1f / 5f;
                    return RadicalInverseOpt((ulong)n, (uint)b, inv5);
                case 7:
                    const float inv7 = 1f / 7f;
                    return RadicalInverseOpt((ulong)n, (uint)b, inv7);
                case 11:
                    const float inv11 = 1f / 11f;
                    return RadicalInverseOpt((ulong)n, (uint)b, inv11);
            }
            throw new NotImplementedException($"Base {b} not implemented.");
        }

        private static float RadicalInverseBase2(ulong n)
        {
            // Magic number below found by the following:
            //var invB = 1 / 2f;
            //var inBN = 1f;
            //for (var i = 0; i < 32; i++)
            //{
            //    inBN *= invB;
            //}
            return ReverseBits64(n) * 2.32830644E-10f; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReverseBits64(ulong n)
        {
            var n0 = ReverseBits32((uint) n);
            var n1 = ReverseBits32((uint) (n >> 32));
            return (n0 << 32) | n1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ReverseBits32(uint n)
        {
            n = (n << 16) | (n >> 16);
            n = ((n & 0x00ff00ffu) << 8) | ((n & 0xff00ff00u) >> 8);
            n = ((n & 0x0f0f0f0fu) << 4) | ((n & 0xf0f0f0f0u) >> 4);
            n = ((n & 0x33333333u) << 2) | ((n & 0xccccccccu) >> 2);
            n = ((n & 0x55555555u) << 1) | ((n & 0xaaaaaaaau) >> 1);
            return n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float RadicalInverseOpt(ulong n, uint b, float invBase)
        {
            ulong reversedDigits = 0;
            float invBaseN = 1f;
            while (n > 0)
            {
                var next = n / b;
                var digit = n - next * b;
                reversedDigits = reversedDigits * b + digit;
                invBaseN *= invBase;
                n = next;
            }
            return MathF.Min(reversedDigits * invBaseN, MathFunction.OneMinusEpsilon);
        }

        //private static double VanDerCorputDouble(long n, int b)
        //{
        //    long p = 0, q = 1;
        //    while (n != 0)
        //    {
        //        p = p * b + n % b;
        //        q *= b;
        //        n /= b;
        //    }

        //    var numerator = p;
        //    var denominator = q;
        //    while (p != 0)
        //    {
        //        n = p;
        //        p = q % p;
        //        q = n;
        //    }

        //    numerator /= q;
        //    denominator /= q;
        //    return numerator / (double)denominator;
        //}

        /// <summary>
        /// Generates Halton sequence pair using the given bases and index number.
        /// </summary>
        /// <param name="p1">Prime number base.</param>
        /// <param name="p2">Primer number base that is not the same as p1.</param>
        /// <param name="i">Index in sequence.</param>
        /// <returns>Pair of low discrepancy, quasi-random values in [0..1].</returns>
        private static UVPoint Gen(int p1, int p2, long i) =>
           new UVPoint(VanDerCorput2(i, p1), VanDerCorput2(i, p2));

        public static UVPoint Next(long index) => Gen(2, 3, index);

        //private static (double, double) GenD(int p1, int p2, long i) =>
        //    (VanDerCorputDouble(100 + i, p1), VanDerCorputDouble(100 + i, p2));

        //public static (double, double) NextD(long index) => GenD(2, 3, index);

        public static float Rand(long index) => VanDerCorput2(100 + index, 5);
    }
}