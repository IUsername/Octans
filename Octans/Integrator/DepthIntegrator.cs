using Octans.Sampling;

namespace Octans.Integrator
{
    public sealed class DepthIntegrator : SamplerIntegrator
    {
        private readonly float _delta;
        private readonly float _maxDist;
        private readonly float _minDist;

        public DepthIntegrator(
            float minDist,
            float maxDist,
            ICamera camera,
            ISampler sampler,
            in PixelArea pixelBounds)
            : base(camera, sampler, in pixelBounds)
        {
            _minDist = minDist;
            _maxDist = maxDist;
            _delta = _maxDist - _minDist;
        }

        protected override void Li(SpectrumAccumulator L, in RayDifferential ray, IScene scene, ISampler tileSampler, IObjectArena arena, int depth = 0)
        {
            var r = ray;
            var si = new SurfaceInteraction();
            if (!scene.Intersect(r, ref si))
            {
                return;// arena.Create<SpectrumAccumulator>().Zero();
            }

            var dist = Point.Distance(si.P, ray.Origin);
            var v = MathF.Clamp(_minDist, _maxDist, dist);
            var x = 1f - (v - _minDist) / _delta;
            L.Contribute(Spectrum.FromRGB(new[] { x, x, x }, SpectrumType.Illuminant));
         //   return arena.Create<SpectrumAccumulator>().FromSpectrum(Spectrum.FromRGB(new[] {x, x, x}, SpectrumType.Illuminant));
        }

        protected override void Preprocess(in IScene scene, ISampler sampler)
        {
        }
    }
}