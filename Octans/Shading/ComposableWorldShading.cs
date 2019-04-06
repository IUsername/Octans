using System.Threading;

namespace Octans.Shading
{
    public class ComposableWorldShading : IWorldShading
    {
        private readonly int _minBounces;
        private readonly World _world;
        private readonly ShadingContext _context;
        private long _index = 0;

        public ComposableWorldShading(int minBounces,
                                      INormalDistribution nd,
                                      IGeometricShadow gs,
                                      IFresnelFunction f,
                                      World world)
        {
            _minBounces = minBounces;
            _world = world;
            _context = new ShadingContext(nd, gs, f);
        }

        public Color ColorFor(in Ray ray)
        {
            var i = Interlocked.Increment(ref _index);
            return _context.ColorAt(_world, in ray, _minBounces, i);
        }
    }

    public class ComposableWorldSampler : IWorldSampler
    {
        private readonly World _world;
        private readonly ShadingContext2 _context;

        public ComposableWorldSampler(int minBounces,
                                      int maxBounces,
                                      INormalDistribution nd,
                                      IGeometricShadow gs,
                                      IFresnelFunction f,
                                      World world)
        {
            _world = world;
            _context = new ShadingContext2(minBounces,maxBounces, nd, gs, f);
        }

        public Color Sample(in Ray ray, ISampler sampler)
        {
            return _context.ColorAt(_world, in ray, sampler);
        }
    }
    
}