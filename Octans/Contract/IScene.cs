﻿using Octans.Primitive;
using Octans.Sampling;

namespace Octans
{
    public interface IScene
    {
        Bounds WorldBounds { get; }
        ILight2[] InfiniteLights { get; }
        ILight2[] Lights { get; }
        IPrimitive Aggregate { get; }
        bool Intersect(Ray r, ref SurfaceInteraction si);
        bool IntersectP(Ray r);
        bool IntersectTr(Ray ray, ISampler2 sampler, ref SurfaceInteraction si, out Spectrum tr);
    }
}