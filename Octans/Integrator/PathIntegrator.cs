using System.Diagnostics;
using Octans.Reflection;
using Octans.Sampling;
using static System.MathF;

namespace Octans.Integrator
{
    public sealed class PathIntegrator : SamplerIntegrator
    {
        private readonly LightSampleStrategy _lightSampleStrategy;
        private readonly float _rrThreshold;
        private LightDistribution _lightDistribution;

        public PathIntegrator(int maxDepth,
                              ICamera camera,
                              ISampler sampler,
                              in PixelArea pixelBounds,
                              float rrThreshold,
                              LightSampleStrategy lightSampleStrategy) 
            : base(camera, sampler, in pixelBounds)
        {
            _rrThreshold = rrThreshold;
            _lightSampleStrategy = lightSampleStrategy;
            MaxDepth = maxDepth;
        }

        public int MaxDepth { get; }

        protected override void Li(SpectrumAccumulator L,
                                   in RayDifferential ray,
                                   IScene scene,
                                   ISampler sampler,
                                   IObjectArena arena,
                                   int depth = 0)
        {
            var specularBounce = false;
            int bounces;
            var beta = Spectrum.One;
            var etaScale = 1f;
            var r = ray;
            var si = arena.Create<SurfaceInteraction>().Reset();

            for (bounces = 0;; ++bounces)
            {
                //si.Reset();
                var foundIntersection = scene.Intersect(r, ref si);

                if (bounces == 0 || specularBounce)
                {
                    if (foundIntersection)
                    {
                        L.Contribute(beta, si.Le(-r.Direction));
                    }
                    else
                    {
                        foreach (var light in scene.InfiniteLights)
                        {
                            L.Contribute(beta, light.Le(in r));
                        }
                    }
                }

                if (!foundIntersection || bounces >= MaxDepth)
                {
                    break;
                }

                si.ComputeScatteringFunctions(r, arena);
                if (si.BSDF.NumberOfComponents() == 0)
                {
                    r = arena.Create<RayDifferential>().Initialize(si.SpawnRay(r.Direction));
                 //   r = new RayDifferential(si.SpawnRay(r.Direction));
                    bounces--;
                    continue;
                }

                var distribution = _lightDistribution.Lookup(si.P);

                const BxDFType nonSpecular = BxDFType.All & ~BxDFType.Specular;
                if (si.BSDF.NumberOfComponents(nonSpecular) > 0)
                {
                    //var Ld = beta * si.UniformSampleOneLight(scene, arena, sampler, false, distribution);
                    //Debug.Assert(Ld.YComponent() >= 0f);
                    //L.Contribute(Ld);

                    L.Contribute(beta, si.UniformSampleOneLight(scene, arena, sampler, false, distribution));
                }

                var wo = -ray.Direction;
                var f = si.BSDF.Sample_F(wo, out var wi, sampler.Get2D(), out var pdf, BxDFType.All, out var flags);
                if (f.IsBlack() || pdf == 0f)
                {
                    break;
                }
               
                //beta *= f * Abs(wi % si.ShadingGeometry.N) / pdf;
                beta = Spectrum.FusedMultiply(beta, f, Abs(wi % si.ShadingGeometry.N) / pdf);
                //beta.Scale(f * (Abs(wi % si.ShadingGeometry.N) / pdf));
                Debug.Assert(beta.YComponent() >= 0f);
                Debug.Assert(!float.IsInfinity(beta.YComponent()));
                specularBounce = (flags & BxDFType.Specular) != BxDFType.None;
                if (flags.HasFlag(BxDFType.Specular) && flags.HasFlag(BxDFType.Transmission))
                {
                    var eta = si.BSDF.Eta;
                    etaScale *= wo % si.N > 0f ? eta * eta : 1f / (eta * eta);
                }

                //r = new RayDifferential(si.SpawnRay(wi));
                r = arena.Create<RayDifferential>().Initialize(si.SpawnRay(wi));

                if (!(si.BSSRDF is null) && flags.HasFlag(BxDFType.Transmission))
                {
                    var pi = arena.Create<SurfaceInteraction>().Reset();
                    var S = si.BSSRDF.SampleS(scene, sampler.Get1D(), sampler.Get2D(), arena, ref pi, out pdf);
                    Debug.Assert(!float.IsInfinity(beta.YComponent()));
                    if (S.IsBlack() || pdf == 0f)
                    {
                        break;
                    }
                    beta *= S / pdf;
                    //beta.Scale(S / pdf);

                    L.Contribute(beta, pi.UniformSampleOneLight(scene, arena, sampler, false,
                                                                 _lightDistribution.Lookup(pi.P)));

                    f = pi.BSDF.Sample_F(pi.Wo, out wi, sampler.Get2D(), out pdf, BxDFType.All, out flags);
                    if (f.IsBlack() || pdf == 0f)
                    {
                        break;
                    }

                    beta = Spectrum.FusedMultiply(beta, f, Abs(wi % pi.ShadingGeometry.N) / pdf);
                    //beta *= f * Abs(wi % pi.ShadingGeometry.N) / pdf;
                    //beta.Scale(f * Abs(wi % pi.ShadingGeometry.N) / pdf);
                    Debug.Assert(!float.IsInfinity(beta.YComponent()));
                    specularBounce = flags.HasFlag(BxDFType.Specular);
                    //r = new RayDifferential(pi.SpawnRay(wi));
                    r = arena.Create<RayDifferential>().Initialize(si.SpawnRay(wi));
                }

                var rrBeta = beta * etaScale;
                if (Spectrum.MaxComponent(rrBeta) < _rrThreshold && bounces > 3)
                {
                    var q = Max(0.05f, 1f - Spectrum.MaxComponent(rrBeta));
                    if (sampler.Get1D() < q)
                    {
                        break;
                    }

                    beta /= 1f - q;
                    //beta.Scale(1f/ (1f - q));
                    Debug.Assert(!float.IsInfinity(beta.YComponent()));
                }
            }
        }

        protected override void Preprocess(in IScene scene, ISampler sampler)
        {
            _lightDistribution = LightDistribution.CreateLightSampleDistribution(_lightSampleStrategy, scene);
        }
    }
}