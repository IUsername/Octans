using System;

namespace Octans.Sampling
{
    public interface ISampler
    {
        UVPoint NextUV();

        float Random();

        ISampler Create(ulong i);
    }
}