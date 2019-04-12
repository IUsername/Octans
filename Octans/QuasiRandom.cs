using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Octans
{
    public static class QuasiRandom
    {
        private static readonly Lazy<IReadOnlyList<ushort>> Perm = new Lazy<IReadOnlyList<ushort>>(
            () => ComputeRadicalInversePermutations(new Random(63)), LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly uint[] PrimesTable = {2, 3, 5, 7, 11};
        private static readonly uint[] PrimesSums = {0, 2, 5, 10, 17};
        private static readonly float[] PrimesInv = {1f / 2, 1f / 3, 1f / 5, 1f / 7, 1f / 11};

        public static IReadOnlyList<ushort> ComputeRadicalInversePermutations(Random rand)
        {
            var permArraySize = 0u;
            for (var i = 0; i < PrimesTable.Length; i++)
            {
                permArraySize += PrimesTable[i];
            }

            var perms = new ushort[permArraySize];
            var index = 0;
            for (var i = 0; i < PrimesTable.Length; i++)
            {
                for (var j = 0; j < PrimesTable[i]; j++)
                {
                    perms[index + j] = (ushort) j;
                }

                var prime =(int) PrimesTable[i];
                Shuffle(perms, index, prime, 1, rand);
                index += prime;
            }

            return perms;
        }

        public static IReadOnlyList<ushort> RadicalInversePermutations() => Perm.Value;

        private static void Shuffle<T>(IList<T> samples, int offset, int count, int nDimensions, Random rand)
        {
            for (var i = 0; i < count; i++)
            {
                var other = i + rand.Next(count - i);
                for (var j = 0; j < nDimensions; j++)
                {
                    var aI = offset + nDimensions * i + j;
                    var bI = offset + nDimensions * other + j;
                    if (aI != bI)
                    {
                        // Swap
                        (samples[aI], samples[bI]) = (samples[bI], samples[aI]);
                    }
                }
            }
        }

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
        /// <param name="dimension">Dimension of the sequence. Determines the prime number base to use for the sequence.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float VanDerCorput2(ulong n, int dimension)
        {
            switch (dimension)
            {
                case 0:
                    //const float inv2 = 1f / 2f;
                    //var rib = RadicalInverseOpt((ulong)n, 2, inv2);
                    return RadicalInverseBase2(n);
                case 1:
                case 2:
                case 3:
                case 4:
                    var b = PrimesTable[dimension];
                    var bInv = PrimesInv[dimension];
                    return RadicalInverseOpt(n, b, bInv);
            }

            throw new NotImplementedException($"Dimension {dimension} not implemented.");
        }

        /// <summary>
        ///     Returns a quasi-random float in [0..1) based on index and base.
        /// </summary>
        /// <param name="n">Index in sequence.</param>
        /// <param name="dimension">Dimension for the sequence.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ScrambledRadicalInverse(ulong n, uint dimension)
        {
            switch (dimension)
            {
                case 0:
                    return RadicalInverseScrambled(n, dimension);
                case 1:
                    return RadicalInverseScrambled(n, dimension);
                case 2:
                    return RadicalInverseScrambled(n, dimension);
                case 3:
                    return RadicalInverseScrambled(n, dimension);
                case 4:
                    return RadicalInverseScrambled(n, dimension);
            }

            throw new NotImplementedException($"Dimension {dimension} not implemented.");
        }

        private static float RadicalInverseBase2(ulong n) => ReverseBits64(n) * 2.32830644E-10f;

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
            var invBaseN = 1f;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float RadicalInverseScrambled(ulong n, uint dimension)
        {
            var b = PrimesTable[dimension];
            var invBase = PrimesInv[dimension];
            var offset = PrimesSums[dimension];
            var permutations = RadicalInversePermutations();
            ulong reversedDigits = 0;
            var invBaseN = 1f;
            while (n > 0)
            {
                var next = n / b;
                var digit = n - next * b;
                reversedDigits = reversedDigits * b + permutations[(int)(offset + digit)];
                invBaseN *= invBase;
                n = next;
            }

            return MathF.Min(invBaseN * (reversedDigits + invBase * permutations[(int)offset] / (1 - invBase)),
                             MathFunction.OneMinusEpsilon);
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
        ///     Generates Halton sequence pair using the given dimensions and index number.
        /// </summary>
        /// <param name="dimU">Dimension for U component.</param>
        /// <param name="dimV">Dimension for V component.</param>
        /// <param name="i">Index in sequence.</param>
        /// <returns>Pair of low discrepancy, quasi-random values in [0..1].</returns>
        private static UVPoint Gen(uint dimU, uint dimV, ulong i)
        {
            var a = new UVPoint(ScrambledRadicalInverse(i, dimU), ScrambledRadicalInverse(i, dimV));
            var b = new UVPoint(VanDerCorput2(i, (int)dimU), VanDerCorput2(i, (int)dimV));
            return a;
        }

        public static UVPoint Next(ulong index) => Gen(0, 1, index);

        //private static (double, double) GenD(int p1, int p2, long i) =>
        //    (VanDerCorputDouble(100 + i, p1), VanDerCorputDouble(100 + i, p2));

        //public static (double, double) NextD(long index) => GenD(2, 3, index);

        public static float Rand(ulong index) => ScrambledRadicalInverse(index, 3);
    }
}