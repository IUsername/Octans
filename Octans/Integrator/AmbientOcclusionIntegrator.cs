using System.Diagnostics;
using Octans.Sampling;
using static System.MathF;
using static Octans.Sampling.Utilities;

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

        protected override Spectrum Li(in RayDifferential ray, IScene scene, ISampler2 tileSampler, IObjectArena arena)
        {
            var L = Spectrum.Zero;
            var r =  ray;
            var hit = false;
            var si = new SurfaceInteraction();
            while (!hit)
            {
                if (!scene.Intersect(r, ref si))
                {
                    return L;
                }

                si.ComputeScatteringFunctions(r, arena, true);
                if (si.BSDF.NumberOfComponents() == 0)
                {
                    r = new RayDifferential(si.SpawnRay(r.Direction));
                    continue;
                }

                hit = true;

                var n = Normal.FaceForward(si.N, -r.Direction);
                var s = si.Dpdu.Normalize();
                var t = Vector.Cross(si.N, s);

                var u = tileSampler.Get2DArray(NSamples);
                //var invL = 1f / NSamples;
                for (var i = 0; i < NSamples; ++i)
                {
                    Vector wi;
                    float pdf;
                    if (CosSample)
                    {
                        wi = CosineSampleHemisphere(u[i]);
                        pdf = CosineHemispherePdf(Abs(wi.Z));
                    }
                    else
                    {
                        wi = UniformSampleHemisphere(u[i]);
                        pdf = UniformHemispherePdf();
                    }

                    // Local to world space
                    wi = new Vector(
                        s.X * wi.X + t.X * wi.Y + n.X * wi.Z,
                        s.Y * wi.X + t.Y * wi.Y + n.Y * wi.Z,
                        s.Z * wi.X + t.Z * wi.Y + n.Z * wi.Z);

                    var nRay = si.SpawnRay(wi);
                    //nRay.TMax = 100f;
                    if (!scene.IntersectP(nRay))
                    {
                        L += (wi % n) / (pdf * NSamples);

                        //L += Spectrum.FromRGB(new []{invL, invL, invL}, SpectrumType.Illuminant);
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