using System;
using System.Linq;
using Octans.Light;
using Octans.Primitive;

namespace Octans.Accelerator
{
    public class BVH : IPrimitive
    {
        private readonly IPrimitive[] _p;

        public BVH(IPrimitive[] p, SplitMethod splitMethod, int maxPerNode = 1)
        {
            // TODO: Implement
            _p = p;
            SplitMethod = splitMethod;
            MaxPerNode = maxPerNode;

            WorldBounds = _p.Aggregate(Bounds.Empty, (current, primitive) => current + primitive.WorldBounds);
        }

        public SplitMethod SplitMethod { get; }
        public int MaxPerNode { get; }

        public Bounds WorldBounds { get; }

        public bool Intersect(ref Ray r, ref SurfaceInteraction si)
        {
            var hit = false;
            for (var index = 0; index < _p.Length; index++)
            {
                var primitive = _p[index];
                if (primitive.Intersect(ref r, ref si))
                {
                    hit = true;
                }
            }

            return hit;
        }

        public bool IntersectP(ref Ray r)
        {
            for (var index = 0; index < _p.Length; index++)
            {
                var primitive = _p[index];
                if (primitive.IntersectP(ref r))
                {
                    return true;
                }
            }

            return false;
        }

        public IMaterial Material { get; }
        public AreaLight AreaLight { get; }

        public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               in bool allowMultipleLobes)
        {
            throw new NotImplementedException();
        }
    }

    public enum SplitMethod
    {
        SAH,
        HLBVH,
        Middle,
        EqualCounts
    }
}