namespace Octans.Shading
{
    public class ComposableWorldShading : IWorldShading
    {
        private readonly int _maxBounces;
        private readonly World _world;
        private readonly ShadingContext _context;

        public ComposableWorldShading(int maxBounces,
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