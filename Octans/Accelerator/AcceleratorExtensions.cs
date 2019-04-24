using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans.Accelerator
{
    public static class AcceleratorExtensions
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint LeftShift3(uint x)
        {
            Debug.Assert(x <= 1 << 10);
            if (x == 1 << 10)
            {
                --x;
            }

            x = (x | (x << 16)) & 0b00000011000000000000000011111111;
            x = (x | (x << 8)) & 0b00000011000000001111000000001111;
            x = (x | (x << 4)) & 0b00000011000011000011000011000011;
            x = (x | (x << 2)) & 0b00001001001001001001001001001001;
            return x;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint EncodeMorton3(this in Vector v)
        {
            Debug.Assert(v.X >= 0f);
            Debug.Assert(v.Y >= 0f);
            Debug.Assert(v.Z >= 0f);
            return (LeftShift3((uint) v.Z) << 2) | (LeftShift3((uint) v.Y) << 1) | LeftShift3((uint) v.X);
        }

        internal static void RadixSort(this MortonPrimitive[] v)
        {
            var temp = new MortonPrimitive[v.Length];
            Array.Copy(v, temp, v.Length);

            const int bitsPerPass = 6;
            const int nBits = 30;
            //Debug.Assert(nBits % bitsPerPass == 0);
            const int nPasses = nBits / bitsPerPass;

            for (var pass = 0; pass < nPasses; ++pass)
            {
                var lowBit = pass * bitsPerPass;
                var @in = (pass & 1) == 1 ? temp : v;
                var @out = (pass & 1) == 1 ? v : temp;

                var nBuckets = 1 << bitsPerPass;
                var bucketCount = new int[nBuckets];
                var bitMask = (1 << bitsPerPass) - 1;

                foreach (var mp in @in)
                {
                    var bucket = (mp.MortonCode >> lowBit) & bitMask;
                    //Debug.Assert(bucket > 0);
                    //Debug.Assert(bucket < nBuckets);
                    ++bucketCount[bucket];
                }

                var outIndex = new int[nBuckets];
                outIndex[0] = 0;
                for (var i = 1; i < nBuckets; ++i)
                {
                    outIndex[i] = outIndex[i - 1] + bucketCount[i - 1];
                }

                foreach (var mp in @in)
                {
                    var bucket = (mp.MortonCode >> lowBit) & bitMask;
                    @out[outIndex[bucket]++] = mp;
                }
            }

            if ((nPasses & 1) == 1)
            {
                Array.Copy(v, temp, v.Length);
            }
        }

        public static int Partition<T>(this T[] A, int left, int right, Predicate<T> pred)
        {
            Debug.Assert(left < right);

            var end = right - 1;
            for (var i = left; i < end;)
            {
                if (pred(A[i]))
                {
                    ++i;
                }
                else
                {
                    A.Swap(i, end);
                    end--;
                }
            }

            return end;
        }

        public static int Partition<T>(this IList<T> A, int left, int right, Predicate<T> pred)
        {
            Debug.Assert(left < right);

            var end = right - 1;
            for (var i = left; i < end;)
            {
                if (pred(A[i]))
                {
                    ++i;
                }
                else
                {
                    A.Swap(i, end);
                    end--;
                }
            }

            return end;
        }

        public static void NthElement<T>(this T[] A, int left, int right, int nth, Comparison<T> comp)
        {
            if (left == right || nth == right)
            {
                return;
            }

            NthElementDepth(A, left, right, nth, comp, UncheckedLog2I(right - left) * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UncheckedLog2I(int v)
        {
            var r = 0;
            while (v > 1)
            {
                v >>= 1;
                r++;
            }

            return r;
        }

        private static void NthElementDepth<T>(this T[] A, int left, int right, int nth, Comparison<T> comp, int depthLimit)
        {
            while (right - left > 3)
            {
                if (depthLimit == 0)
                {
                    // TODO: Look into using Median of Medians here.
                    A.HeapSelect(nth + 1, left, right, comp);
                    A.Swap(left, nth);
                    return;
                }

                --depthLimit;
                var mid = A[left + (right - left) / 2];
                var cut = A.Partition(left, right, i => comp(i, mid) < 0);
                if (cut <= nth)
                {
                    left = cut;
                }
                else
                {
                    right = cut;
                }
            }

            InsertionSort(A, left, right, comp);
        }

        private static void InsertionSort<T>(IList<T> A, in int left, in int right, Comparison<T> comp)
        {
            var i = left + 1;
            while (i < right)
            {
                var x = A[i];
                var j = i - 1;
                while (j >= left && A[j].IsGreaterThan(x, comp))
                {
                    A[j + 1] = A[j];
                    j--;
                }

                A[j + 1] = x;
                i++;
            }
        }

        //public static void MaxHeapify<T>(this T[] A, int i, Comparison<T> comp)
        //{
        //    while (true)
        //    {
        //        var left = (i << 1) + 1;
        //        var right = left + 1;
        //        var largest = i;

        //        if (left < A.Length && A[left].IsGreaterThan(A[largest], comp))
        //        {
        //            largest = left;
        //        }

        //        if (right < A.Length && A[right].IsGreaterThan(A[largest], comp))
        //        {
        //            largest = right;
        //        }

        //        if (largest != i)
        //        {
        //            A.Swap(i, largest);
        //            i = largest;
        //            continue;
        //        }

        //        break;
        //    }
        //}

        //public static void BuildMaxHeap<T>(this T[] A, Comparison<T> comp)
        //{
        //    var start = A.Length / 2;
        //    for (var i = start; i > -1; --i)
        //    {
        //        MaxHeapify(A, i, comp);
        //    }
        //}

        //public static void MinHeapify<T>(this T[] A, int i, Comparison<T> comp)
        //{
        //    while (true)
        //    {
        //        var left = (i << 1) + 1;
        //        var right = left + 1;
        //        var smallest = i;

        //        if (left < A.Length && A[left].IsLessThan(A[smallest], comp))
        //        {
        //            smallest = left;
        //        }

        //        if (right < A.Length && A[right].IsLessThan(A[smallest], comp))
        //        {
        //            smallest = right;
        //        }

        //        if (smallest != i)
        //        {
        //            A.Swap(i, smallest);
        //            i = smallest;
        //            continue;
        //        }

        //        break;
        //    }
        //}

        //public static void BuildMinHeap<T>(this T[] A, Comparison<T> comp)
        //{
        //    var start = A.Length / 2;
        //    for (var i = start; i > -1; --i)
        //    {
        //        MinHeapify(A, i, comp);
        //    }
        //}

        public static T HeapSelect<T>(this T[] A, int k, int first, int last, Comparison<T> comp)
        {
          //  var last = A.Length - 1;
            int youngestParent = last / 2;
            for (var i = youngestParent; i >= first; --i)
            {
                MoveDown(A, i, last, comp);
            }

            int limit = last - k+1;
            
            for (var i = last; i > limit; --i)
            {
                if (A[first].IsGreaterThan(A[i], comp))
                {
                    A.Swap(first, i);
                    MoveDown(A, first, i-1, comp);
                }
            }

            return A[first];
        }

        private static void MoveDown<T>(IList<T> A, int first, in int last, Comparison<T> comp)
        {
            var largest = (first << 1) + 1;
            while (largest <= last)
            {
                if (largest < last && A[largest].IsLessThan(A[largest + 1], comp))
                {
                    largest++;
                }

                if (A[first].IsLessThan(A[largest], comp))
                {
                    A.Swap(first, largest);
                    first = largest;
                    largest = (first << 1) + 1;
                }
                else
                {
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLessThan<T>(this T a, in T b, Comparison<T> comp) => comp(a, b) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsGreaterThan<T>(this T a, in T b, Comparison<T> comp) => comp(a, b) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(this IList<T> A, in int i, in int j)
        {
            var temp = A[i];
            A[i] = A[j];
            A[j] = temp;
        }
    }
}