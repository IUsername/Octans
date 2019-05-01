using Octans.Sampling;
using static System.MathF;

namespace Octans.Integrator
{
    public sealed class WhittedIntegrator : SamplerIntegrator
    {
        private readonly int _maxDepth;

        public WhittedIntegrator(int maxDepth, ICamera camera, ISampler sampler, in PixelArea pixelBounds) : base(
            camera, sampler, in pixelBounds)
        {
            _maxDepth = maxDepth;
        }

        protected override Spectrum Li(
                                   in RayDifferential ray,
                                   IScene scene,
                                   ISampler sampler,
                                   IObjectArena arena,
                                   int depth = 0)
        {
            var cr = ray;
            var si = new SurfaceInteraction();
            var L = Spectrum.Zero;
            while (true)
            {
                if (!scene.Intersect(cr, ref si))
                {
                    foreach (var light in scene.Lights)
                    {
                        L += light.Le(cr);
                    }

                    return L;
                }

                var n = si.ShadingGeometry.N;
                var wo = si.Wo;

                si.ComputeScatteringFunctions(cr, arena);
                if (si.BSDF.NumberOfComponents() == 0)
                {
                    cr = new RayDifferential(si.SpawnRay(cr.Direction));
                    continue;
                }

                L += si.Le(wo);

                foreach (var light in scene.Lights)
                {
                    var Li = light.Sample_Li(si, sampler.Get2D(), out var wi, out var pdf, out var visibility);
                    if (Li.IsBlack() || pdf == 0f)
                    {
                        continue;
                    }

                    var f = si.BSDF.F(in wo, in wi);
                    if (!f.IsBlack() && visibility.Unoccluded(scene))
                    {
                        L += f * Li * (Abs(wi % n) / pdf);
                    }
                }

                if (depth + 1 < _maxDepth)
                {
                    L += SpecularReflect(cr, si, scene, sampler, arena, depth);
                    L += SpecularTransmit(cr, si, scene, sampler, arena, depth);
                }

                return L;
            }
        }

        protected override void Preprocess(in IScene scene, ISampler sampler)
        {
        }
    }
}