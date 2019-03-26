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

    public class WorldShadingContext : IWorldShading
    {
        private readonly int _maxBounces;
        private readonly World _world;
        private readonly ShadingContext _context;

        public WorldShadingContext(int maxBounces,
                                   INormalDistribution nd,
                                   IGeometricShadow gs,
                                   IFresnelFunction f,
                                   World world)
        {
            _maxBounces = maxBounces;
            _world = world;
            _context = new ShadingContext(nd, gs, f);
        }

        public Color ColorFor(in Ray ray)
        {
            return _context.ColorAt(_world, in ray, _maxBounces);
        }
    }
}