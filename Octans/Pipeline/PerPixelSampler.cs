using System;

namespace Octans.Pipeline
{
    public sealed class PerPixelSampler : IPixelSampler
    {
        public PerPixelSampler(int samplesPerPixel)
        {
            SamplesPerPixel = samplesPerPixel;
        }

        public int SamplesPerPixel { get; }

        public Color Gather(in PixelInformation pixel, ISampler sampler, IPixelRenderSegment segment)
        {
            var total = Colors.Black;
            var index = Math.Abs(pixel.Coordinate.GetHashCode());
            var local = sampler.Create(index);
            for (var i = 0; i < SamplesPerPixel; i++)
            {
                var (u, v) = local.NextUV();
                var sample = new PixelSample(in pixel, u, v);
                total += segment.Render(in sample, sampler.Create(Index(in sample)));
            }
            return total / SamplesPerPixel;
        }

        private static int Index(in PixelSample sample)
        {
            var hashCode = sample.Pixel.Coordinate.X;
            hashCode = (hashCode * 397) ^ sample.Pixel.Coordinate.Y;
            hashCode = (hashCode * 397) ^ sample.U.GetHashCode();
            hashCode = (hashCode * 397) ^ sample.V.GetHashCode();
            return Math.Abs(hashCode);
        }
    }
}