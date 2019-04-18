namespace Octans
{
    public interface ITexture2<out T>
    {
        T Evaluate(in SurfaceInteraction si);
    }
}