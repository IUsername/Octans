using System.Linq;
using Octans.Reflection;
using Octans.Sampling;

namespace Octans.Integrator
{
    //public sealed class PathIntegrator : SamplerIntegrator
    //{
    //    private LightDistributions _lightDistributions;
    //    public int MaxDepth { get; }

    //    public PathIntegrator(int maxDepth, ICamera camera, ISampler2 sampler, in PixelArea pixelBounds) : base(camera, sampler, in pixelBounds)
    //    {
    //        MaxDepth = maxDepth;
    //    }

    //    protected override Spectrum Li(in Ray ray, IScene scene, ISampler2 tileSampler, IObjectArena arena)
    //    {
    //        var L = Spectrum.Zero;
    //        bool specularBounce = false;
    //        int bounces;
    //        var beta = 1f;
    //        var etaScale = 1f;
    //        var r = ray;

    //        for (bounces = 0;; ++bounces)
    //        {
    //            var foundIntersection = scene.Intersect(r, out var si);

    //            if (bounces == 0 || specularBounce)
    //            {
    //                if (foundIntersection)
    //                {
    //                    L += beta * si.Le(-r.Direction);
    //                }
    //                else
    //                {
    //                    L = scene.InfiniteLights.Aggregate(L, (current, light) => current + beta * light.Le(r));
    //                }
    //            }

    //            if (!foundIntersection || bounces >= MaxDepth) break;

    //            si.ComputeScatteringFunctions(r, arena, true);
    //            if (si.BSDF.NumberOfComponents() == 0)
    //            {
    //                r = si.SpawnRay(r.Direction);
    //                bounces--;
    //                continue;
    //            }

    //            var distribution = _lightDistributions.Lookup(si.P);

    //            const BxDFType nonSpecular = BxDFType.All & ~BxDFType.Specular;
    //            if (si.BSDF.NumberOfComponents(nonSpecular) > 0)
    //            {
                  
    //            }

    //        }
    //    }

    //    protected override void Preprocess(in Scene scene, ISampler2 sampler)
    //    {
    //        _lightDistributions = CreateLightDistribution(, scene);
    //    }
    //}

    //internal class LightDistributions
    //{
    //    public object Lookup(in Point p)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}