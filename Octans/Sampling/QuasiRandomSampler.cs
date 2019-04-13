using System;
using System.Threading;

namespace Octans.Sampling
{
    public class QuasiRandomSampler : ISampler
    {
        private static readonly Lazy<ushort[]> LazyPermutations = new Lazy<ushort[]>(
            () => QuasiRandom.ComputeRadicalInversePermutations(new Random(63)),
            LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly ushort[] _dim0Perms;
        private readonly ushort[] _dim1Perms;
        private readonly ushort[] _dim2Perms;
        private ulong _index;
        private ulong _rInd;

        public QuasiRandomSampler(ulong index)
        {
            _index = index;
            _rInd = index;

            _dim0Perms = QuasiRandom.PermutationsForDimension(RadicalInversePermutations, 0).ToArray();
            _dim1Perms = QuasiRandom.PermutationsForDimension(RadicalInversePermutations, 1).ToArray();
            _dim2Perms = QuasiRandom.PermutationsForDimension(RadicalInversePermutations, 2).ToArray();
        }


        private static ushort[] RadicalInversePermutations => LazyPermutations.Value;

        public UVPoint NextUV()
        {
            var i = ++_index;
            return new UVPoint(QuasiRandom.RadicalInverseScrambled(i, 0, _dim0Perms),
                               QuasiRandom.RadicalInverseScrambled(i, 1, _dim1Perms));
        }

        public float Random() => QuasiRandom.RadicalInverseScrambled(_rInd++, 2, _dim2Perms);

        public ISampler Create(ulong i) => new QuasiRandomSampler(i);
    }
}