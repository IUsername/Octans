using Octans.Sampling;

namespace Octans
{
    public class QuasiRandomSampler : ISampler
    {
        private ulong _index;
        private ulong _rInd;

        public QuasiRandomSampler(ulong index)
        {
            _index = index;
            _rInd = index;
        }

        public UVPoint NextUV()
        {
            return QuasiRandom.Next(++_index);
        }

        public float Random()
        {
            return QuasiRandom.Rand(_rInd++);
        }

        public ISampler Create(ulong i)
        { 
            return new QuasiRandomSampler(i);
        }
    }
}