using System;

namespace Octans.Sampling
{
    public class RandomSampler : SamplerBase
    {
        private readonly Random _rand;

        public RandomSampler(long samplesPerPixel, int seed) : base(samplesPerPixel)
        {
            _rand = new Random(seed);
        }

        private RandomSampler(RandomSampler other, int seed) : base(other.SamplesPerPixel)
        {
            _rand = new Random(seed);

            foreach (var row in other.Samples1D)
            {
                Samples1D.Add(new float[row.Length]);
            }

            foreach (var row in other.Samples2D)
            {
                Samples2D.Add(new Point2D[row.Length]);
            }
        }

        public override Point2D Get2D() => new Point2D((float) _rand.NextDouble(), (float) _rand.NextDouble());

        public override float Get1D() => (float) _rand.NextDouble();

        public override ISampler2 Clone(int seed) => new RandomSampler(this, seed);

        public override void StartPixel(in PixelCoordinate p)
        {
            Compute1DSamples();
            Compute2DSamples();
            base.StartPixel(in p);
        }

        private void Compute2DSamples()
        {
            for (var i = 0; i < Samples2D.Count; i++)
            {
                var array = Samples2D[i];
                for (var j = 0; j < array.Length; j++)
                {
                    array[j] = new Point2D((float) _rand.NextDouble(), (float) _rand.NextDouble());
                }
            }
        }

        private void Compute1DSamples()
        {
            for (var i = 0; i < Samples1D.Count; i++)
            {
                var array = Samples1D[i];
                for (var j = 0; j < array.Length; j++)
                {
                    array[j] = (float) _rand.NextDouble();
                }
            }
        }
    }
}