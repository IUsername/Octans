using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Octans.Sampling
{
    public static class QuasiRandom
    {
        private static readonly uint[] PrimesTable = {2, 3, 5, 7, 11, 13};
        private static readonly uint[] PrimesSums = {0, 2, 5, 10, 17, 28};
        private static readonly float[] PrimesInv = {1f / 2, 1f / 3, 1f / 5, 1f / 7, 1f / 11, 1f / 13};

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

            return MathF.Min(reversedDigits * invBaseN, MathFunction.OneMinusEpsilon);
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

            return MathF.Min(invBaseN * (reversedDigits + invBase * permutations[0] / (1 - invBase)),
                             MathFunction.OneMinusEpsilon);
        }
    }
}