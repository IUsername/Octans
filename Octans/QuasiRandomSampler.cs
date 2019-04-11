namespace Octans
{
    public class QuasiRandomSampler : ISampler
    {
        private long _index;
        private long _rInd;

        public QuasiRandomSampler(long index)
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

        public ISampler Create(long i)
        { 
            return new QuasiRandomSampler(i);
        }
    }
}