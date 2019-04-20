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
            var x = 1f - (v - _minDist) / _delta;
            return Spectrum.FromRGB(new[] {x, x, x}, SpectrumType.Illuminant);
        }

        protected override void Preprocess(in Scene scene, ISampler2 sampler)
        {
        }
    }
}