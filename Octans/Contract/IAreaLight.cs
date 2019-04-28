namespace Octans
{
    public interface IAreaLight : ILight
    {
        Spectrum L(SurfaceInteraction si, in Vector w);
    }
}