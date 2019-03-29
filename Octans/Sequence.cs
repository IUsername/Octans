using System;
using System.Threading;

namespace Octans
{
    public class Sequence
    {
        private readonly float[] _d;
        private readonly int _last;
        private int _position = -1;

        public Sequence(params float[] d)
        {
            _d = d;
            _last = d.Length - 1;
        }

        public float Next()
        {
            var pos = Interlocked.Increment(ref _position);
            if (pos > _last)
            {
                _position = -1;
                return _d[0];
            }

            return _d[pos];
        }

        /// <summary>
        ///     Returns new instance of small approximate Poisson disc values from 0 to +1
        /// </summary>
        /// <returns></returns>
        public static Sequence SmallZeroPosOne() => new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f);

        /// <summary>
        ///     Larger, random distribution of values from -1 to +1;
        /// </summary>
        /// <returns></returns>
        public static Sequence LargeRandomUnit()
        {
            var random = new Random(17);
            var count = 293;
            var keys = new float[count];
            for (var i = 0; i < count; i++)
            {
                keys[i] = random.Next(-10000, 10000) / 10000f;
            }

            return new Sequence(keys);
        }

        public static Sequence LargeRandomZeroOne()
        {
            var random = new Random(17);
            var count = 1024;
            var keys = new float[count];
            for (var i = 0; i < count; i++)
            {
                keys[i] = (float) random.NextDouble();
            }

            return new Sequence(keys);
        }
    }

    public class Sequence<T>
    {
        private readonly T[] _d;
        private readonly int _last;
        private int _position = -1;

        public Sequence(params T[] d)
        {
            _d = d;
            _last = d.Length - 1;
        }

        public T Next()
        {
            var pos = Interlocked.Increment(ref _position);
            if (pos > _last)
            {
                _position = -1;
                return _d[0];
            }

            return _d[pos];
        }
    }
}