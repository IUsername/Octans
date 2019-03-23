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
        ICameraPosition CameraPosition { get; }
        IWorldShading World { get; }
    }

    public interface ICameraPosition
    {
        Matrix Transform { get; }
        Matrix TransformInverse { get; }
    }

    public class RaytracedWorld : IWorldShading
    {
        private readonly World _world;

        public int MaxBounces { get; }

        public RaytracedWorld(int maxBounces, World world)
        {
            _world = world;
            MaxBounces = maxBounces;
        }

        public Color ColorFor(in Ray ray)
        {
            return Shading.ColorAt(_world, in ray, MaxBounces);
        }
    }
}