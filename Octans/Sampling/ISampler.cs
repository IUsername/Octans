using System;

namespace Octans.Sampling
{
    public interface ISampler
    {
        UVPoint NextUV();

        float Random();

        ISampler Create(ulong i);
    }

    public class CameraSample
    {
        public Point2D FilmPoint { get; set; }
        public Point2D LensPoint { get; set; }
    }
}