using Octans.Sampling;

namespace Octans.Integrator
{
    public sealed class NormalIntegrator : SamplerIntegrator
    {
        public NormalIntegrator(
            ICamera camera,
            ISampler2 sampler,
            in PixelArea pixelBounds)
            : base(camera, sampler, in pixelBounds)
        {
        }

        protected override Spectrum Li(in RayDifferential ray, IScene scene, ISampler2 tileSampler, IObjectArena arena)
        {
            var r = ray;
            var si = new SurfaceInteraction();
            if (!scene.Intersect(r, ref si))
            {
                return Spectrum.Zero;
            }

            var n = (si.N + new Normal(1f, 1f, 1f)) * 0.5f;
            var rgb = new float[3];
            rgb[0] = Utilities.InvGammaCorrect(n.X);
            rgb[1] = Utilities.InvGammaCorrect(n.Y);
            rgb[2] = Utilities.InvGammaCorrect(1f - n.Z);

            return Spectrum.FromRGB(rgb, SpectrumType.Reflectance);
        }

        protected override void Preprocess(in Scene scene, ISampler2 sampler)
        {
        }
    }
}