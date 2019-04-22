using Octans.Primitive;

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
    }
}