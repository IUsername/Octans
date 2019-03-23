namespace Octans
{
    public sealed class Scene : IPixelRenderer, IScene
    {
        private readonly ICamera _camera;

        public Scene(ICamera camera, ICameraPosition cameraPosition, IWorldShading worldShading)
        {
            _camera = camera;
            CameraPosition = cameraPosition;
            World = worldShading;
        }

        public Color Render(in SubPixel sp)
        {
            return _camera.Render(this, in sp);
        }

        public ICameraPosition CameraPosition { get; }
        public IWorldShading World { get; }
    }
}