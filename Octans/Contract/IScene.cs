using Octans.Primitive;

namespace Octans
{
    public interface IScene
    {
        Bounds WorldBounds { get; }
        ILight[] InfiniteLights { get; }
        ILight[] Lights { get; }
        IPrimitive Aggregate { get; }
        bool Intersect(ref Ray r, ref SurfaceInteraction si);
        bool IntersectP(ref Ray r);
    }
}