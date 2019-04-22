using Octans.Light;

namespace Octans.Primitive
{
    public interface IPrimitive
    {
        Bounds WorldBounds { get; }

        bool Intersect(ref Ray r, ref SurfaceInteraction si);

        bool IntersectP(ref Ray r);

        IMaterial Material { get; }

        IAreaLight AreaLight { get; }

        void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                        IObjectArena arena,
                                        TransportMode mode,
                                        in bool allowMultipleLobes);
    }
}