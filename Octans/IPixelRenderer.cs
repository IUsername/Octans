namespace Octans
{
    public interface IPixelRenderer
    {
        Color Render(in SubPixel sp);
    }

    public interface ICamera
    {
        Color Render(IScene scene, in SubPixel sp);
    }

    public interface IWorldShading
    {
        Color ColorFor(in Ray ray);
    }

    public interface IScene
    {
        IWorldShading World { get; }
    }
}