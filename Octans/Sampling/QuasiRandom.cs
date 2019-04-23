using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Octans.Sampling
{
    public static class QuasiRandom
    {
        private static readonly uint[] PrimesTable =
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103,
            107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223,
            227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347,
            349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
            467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607,
            613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743,
            751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883,
            887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019, 1021, 1031,
            1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153,
            1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289,
            1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409, 1423, 1427, 1429, 1433,
            1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553,
            1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669,
            1693, 1697, 1699, 1709, 1721, 1723, 1733
        };

        private static readonly uint[] PrimesSums;
        private static readonly float[] PrimesInv;

        static QuasiRandom()
        {
            PrimesSums = new uint[PrimesTable.Length];
            PrimesInv = new float[PrimesTable.Length];

            var prior = 0u;
            var current = 0u;
            for (var i = 0; i < PrimesTable.Length; ++i)
            {
                prior += current;
                PrimesSums[i] = prior;

                current = PrimesTable[i];
                PrimesInv[i] = 1f / current;
            }
        }

        //private static readonly uint[] PrimesSums =
        //    {0, 2, 5, 10, 17, 28, 41, 58, 77, 100, 129, 160, 197, 238, 281, 328, 381};

        //private static readonly float[] PrimesInv =
        //{
        //    1f / 2, 1f / 3, 1f / 5, 1f / 7, 1f / 11, 1f / 13, 1f / 17, 1f / 19, 1f / 23, 1f / 29, 1f / 31, 1f / 37,
        //    1f / 41, 1f / 43, 1f / 47, 1f / 53, 1f / 59
        //};

        public static ushort[] ComputeRadicalInversePermutations(Random rand)
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

                var prime = (int) PrimesTable[i];
                Shuffle(perms, index, prime, 1, rand);
                index += prime;
            }

            return perms;
        }

        public static ReadOnlySpan<ushort> PermutationsForDimension(ushort[] permutations, int dim)
        {
            Debug.Assert(dim < PrimesTable.Length, $"Can only sample {PrimesTable.Length} dimensions.");
            return new ReadOnlySpan<ushort>(permutations, (int) PrimesSums[dim], (int) PrimesTable[dim]);
        }

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

        // TODO: Use ScaleB?
        public static float RadicalInverseBase2(ulong n) => ReverseBits64(n) * 2.32830644E-10f;

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
        public static float RadicalInverse(ulong n, int dimension)
        {
            Debug.Assert(dimension < PrimesTable.Length);
            var b = PrimesTable[dimension];
            var bInv = PrimesInv[dimension];
            ulong reversedDigits = 0;
            var invBaseN = 1f;
            while (n > 0)
            {
                var next = n / b;
                var digit = n - next * b;
                reversedDigits = reversedDigits * b + digit;
                invBaseN *= bInv;
                n = next;
            }

            return System.MathF.Min(reversedDigits * invBaseN, MathF.OneMinusEpsilon);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadicalInverseScrambled(ulong n, int dimension, ReadOnlySpan<ushort> permutations)
        {
            Debug.Assert(dimension < PrimesTable.Length);
            var b = PrimesTable[dimension];
            var invBase = PrimesInv[dimension];
            ulong reversedDigits = 0;
            var invBaseN = 1f;
            while (n > 0)
            {
                var next = n / b;
                var digit = n - next * b;
                reversedDigits = reversedDigits * b + permutations[(int) digit];
                invBaseN *= invBase;
                n = next;
            }

            return System.MathF.Min(invBaseN * (reversedDigits + invBase * permutations[0] / (1 - invBase)),
                                    MathF.OneMinusEpsilon);
        }
    }
}