using Octans.Sampling;

namespace Octans.Integrator
{
    public sealed class AmbientOcclusionIntegrator : SamplerIntegrator
    {
        public AmbientOcclusionIntegrator(bool cosSample,
                                          int nSamples,
                                          ICamera camera,
                                          ISampler2 sampler,
                                          in PixelArea pixelBounds)
            : base(camera, sampler, in pixelBounds)
        {
            CosSample = cosSample;
            NSamples = sampler.RoundCount(nSamples);
            sampler.Request2DArray(NSamples);
        }


        public bool CosSample { get; }
        public int NSamples { get; }

        protected override Spectrum Li(in Ray ray, IScene scene, ISampler2 tileSampler, IObjectArena arena)
        {
            var L = Spectrum.Zero;
            var r = ray;
            var hit = false;
            while (!hit)
            {
                if (!scene.Intersect(r, out var si))
                {
                    return L;
                }

                si.ComputeScatteringFunctions(r, arena, true);
                if (si.BSDF.NumberOfComponents() == 0)
                {
                    r = si.SpawnRay(r.Direction);
                    continue;
                }

                hit = true;

                var n = Normal.FaceForward(si.N, -r.Direction);
                var s = si.Dpdu.Normalize();
                var t = Vector.Cross(si.N, s);

                var u = tileSampler.Get2DArray(NSamples);
                for (var i = 0; i < NSamples; ++i)
                {
                    Vector wi;
                    float pdf;
                    if (CosSample)
                    {
                        wi = Sampling.Utilities.CosineSampleHemisphere(u[i]);
                        pdf = Sampling.Utilities.CosineHemispherePdf(System.MathF.Abs(wi.Z));
                    }
                    else
                    {
                        wi = Sampling.Utilities.UniformSampleHemisphere(u[i]);
                        pdf = Sampling.Utilities.UniformSampleHemispherePdf();
                    }

                    wi = new Vector(
                        s.X * wi.X + t.X * wi.Y + n.X * wi.Z,
                        s.Y * wi.X + t.Y * wi.Y + n.Y * wi.Z,
                        s.Z * wi.X + t.Z * wi.Y + n.Z * wi.Z);

                    if (!scene.IntersectP(si.SpawnRay(wi)))
                    {
                        L += wi % n / (pdf * NSamples);
                    }
                }
            }

            return L;
        }

        protected override void Preprocess(in Scene scene, ISampler2 sampler)
        {
        }
    }
}