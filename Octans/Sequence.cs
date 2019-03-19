using System.Threading;

namespace Octans
{
    public class Sequence
    {
        private readonly float[] _d;
        private int _position = -1;

        public Sequence(params float[] d)
        {
            _d = d;
        }

        public float Next()
        {
            var pos = Interlocked.Increment(ref _position);
            return _d[pos % _d.Length];
        }
    }
}