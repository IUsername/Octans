using Octans.Sampling;

namespace Octans.Integrator
{
    public sealed class DepthIntegrator : SamplerIntegrator
    {
        private readonly float _maxDist;
        private readonly float _minDist;
        private readonly float _delta;

        public DepthIntegrator(
            float minDist,
            float maxDist,
            ICamera camera,
            ISampler2 sampler,
            in PixelArea pixelBounds)
            : base(camera, sampler, in pixelBounds)
        {
            _minDist = minDist;
            _maxDist = maxDist;
            _delta = _maxDist - _minDist;
        }

        protected override Spectrum Li(in Ray ray, IScene scene, ISampler2 tileSampler, IObjectArena arena)
        {
            var r = ray;
            var si = new SurfaceInteraction();
            if (!scene.Intersect(ref r, ref si))
            {
                return Spectrum.Zero;
            }

            var dist = Point.Distance(si.P, ray.Origin);
            var v = MathF.Clamp(_minDist, _maxDist, dist);
            var x = MathF.Clamp(0, 1, (v - _minDist) / _delta);
            return new Spectrum(1f - x);
        }

        protected override void Preprocess(in Scene scene, ISampler2 sampler)
        {
        }
    }
}