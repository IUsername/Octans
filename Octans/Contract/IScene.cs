namespace Octans
{
    public interface IScene
    {
        Bounds WorldBounds { get; }
        ILight[] InfiniteLights { get; }
        ILight[] Lights { get; }
        IGeometry Aggregate { get; }
        bool Intersect(in Ray r, out SurfaceInteraction si);
        bool IntersectP(in Ray r);
    }
}