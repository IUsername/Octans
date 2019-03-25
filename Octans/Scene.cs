namespace Octans
{
    public sealed class Scene : IPixelRenderer, IScene
    {
        private readonly ICamera _camera;

        public Scene(ICamera camera, IWorldShading worldShading)
        {
            _camera = camera;
            World = worldShading;
        }

        public Color Render(in SubPixel sp)
        {
            return _camera.Render(this, in sp);
        }

        public IWorldShading World { get; }
    }
}