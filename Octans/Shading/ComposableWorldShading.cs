using Octans.Sampling;

namespace Octans.Shading
{
    public class ComposableWorldSampler : IWorldSampler
    {
        private readonly World _world;
        private readonly ShadingContext _context;

        public ComposableWorldSampler(int minBounces,
                                      int maxBounces,
                                      INormalDistribution nd,
                                      IGeometricShadow gs,
                                      IFresnelFunction f,
                                      World world)
        {
            _world = world;
            _context = new ShadingContext(minBounces,maxBounces, nd, gs, f);
        }

        public Color Sample(in Ray ray, ISampler sampler)
        {
            return _context.ColorAt(_world, in ray, sampler);
        }
    }
    
}