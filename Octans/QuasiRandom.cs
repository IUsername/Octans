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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float VanDerCorput(long n, int b)
        {
            float p = 0, q = 1;
            while (n > 0)
            {
                p += (n % b) / (q *= b);
                n /= b;
            }

            return p;
        }

        private static double VanDerCorputDouble(long n, int b)
        {
            long p = 0, q = 1;
            while (n != 0)
            {
                p = p * b + n % b;
                q *= b;
                n /= b;
            }

            var numerator = p;
            var denominator = q;
            while (p != 0)
            {
                n = p;
                p = q % p;
                q = n;
            }

            numerator /= q;
            denominator /= q;
            return numerator / (double)denominator;
        }

        /// <summary>
        /// Generates Halton sequence pair using the given bases and index number.
        /// </summary>
        /// <param name="p1">Prime number base.</param>
        /// <param name="p2">Primer number base that is not the same as p1.</param>
        /// <param name="i">Index in sequence.</param>
        /// <returns>Pair of low discrepancy, quasi-random values in [0..1].</returns>
        private static (float, float) Gen(int p1, int p2, long i) =>
            (VanDerCorput(100 + i, p1), VanDerCorput(100 + i, p2));

        public static (float, float) Next(long index) => Gen(2, 3, index);

        private static (double, double) GenD(int p1, int p2, long i) =>
            (VanDerCorputDouble(100 + i, p1), VanDerCorputDouble(100 + i, p2));

        public static (double, double) NextD(long index) => GenD(2, 3, index);

        public static float Rand(long index) => VanDerCorput(100 + index, 5);
    }
}