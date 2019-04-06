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
            var local = sampler.Create(Math.Abs(pixel.Coordinate.GetHashCode()));
            // Needs to have the precision of double.
            var width = (double) pixel.Width;
            var height = (double) pixel.Height;
            for (var i = 0; i < SamplesPerPixel; i++)
            {
                var (u, v) = local.NextUV();
                var iu = (pixel.Coordinate.X + u) / width;
                var iv = (pixel.Coordinate.Y + v) / height;
                var hash = Math.Abs(iu.GetHashCode() + iv.GetHashCode());
                total += segment.Render(new PixelSample(pixel.Coordinate, iu, iv), sampler.Create(hash));
            }

            return total / SamplesPerPixel;
        }
    }
}