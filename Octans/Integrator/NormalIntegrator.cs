using Octans.Sampling;

namespace Octans.Integrator
{
    public sealed class NormalIntegrator : SamplerIntegrator
    {
        public NormalIntegrator(
            ICamera camera,
            ISampler sampler,
            in PixelArea pixelBounds)
            : base(camera, sampler, in pixelBounds)
        {
        }

        protected override void Li(SpectrumAccumulator L, in RayDifferential ray,
                                       IScene scene,
                                       ISampler tileSampler,
                                       IObjectArena arena,
                                       int depth = 0)
        {
            var r = ray;
            var si = new SurfaceInteraction();
            if (!scene.Intersect(r, ref si))
            {
                return;// arena.Create<SpectrumAccumulator>().Zero();
            }

            var n = (si.N + new Normal(1f, 1f, 1f)) * 0.5f;
            var rgb = new float[3];
            rgb[0] = Utilities.InvGammaCorrect(n.X);
            rgb[1] = Utilities.InvGammaCorrect(n.Y);
            rgb[2] = Utilities.InvGammaCorrect(1f - n.Z);

            L.Contribute(Spectrum.FromRGB(rgb, SpectrumType.Reflectance));
           // return arena.Create<SpectrumAccumulator>().FromSpectrum(Spectrum.FromRGB(rgb, SpectrumType.Reflectance));
        }

        protected override void Preprocess(in IScene scene, ISampler sampler)
        {
        }
    }
}