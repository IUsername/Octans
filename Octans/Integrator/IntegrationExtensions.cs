using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Octans.Reflection;
using Octans.Sampling;
using static Octans.Sampling.Utilities;

namespace Octans.Integrator
{
    public static class IntegrationExtensions
    {
        public static Spectrum UniformSampleOneLight(this Interaction it,
                                                     IScene scene,
                                                     IObjectArena arena,
                                                     ISampler sampler,
                                                     bool handleMedia = false,
                                                     Distribution1D lightDistribution = null)
        {
            var nLights = scene.Lights.Length;
            if (nLights == 0)
            {
                return Spectrum.Zero;
            }

            int lightNum;
            float lightPdf;
            if (!(lightDistribution is null))
            {
                lightNum = lightDistribution.SampleDiscrete(sampler.Get1D(), out lightPdf, out _);
                //if (lightNum == 0)
                //{
                //    return Spectrum.Zero;
                //}
            }
            else
            {
                lightNum = System.Math.Min((int) (sampler.Get1D() * nLights), nLights - 1);
                lightPdf = 1f / nLights;
            }

            var light = scene.Lights[lightNum];
            var uLight = sampler.Get2D();
            var uScattering = sampler.Get2D();
            return EstimateDirect(it, uScattering, light, uLight, scene, sampler, arena, handleMedia) / lightPdf;
        }

        private static Spectrum EstimateDirect(Interaction it,
                                               in Point2D uScattering,
                                               ILight light,
                                               in Point2D uLight,
                                               IScene scene,
                                               ISampler sampler,
                                               IObjectArena arena,
                                               in bool handleMedia = false,
                                               bool specular = false)
        {
            var bsdfFlags = specular ? BxDFType.All : BxDFType.All & ~BxDFType.Specular;

            var scatteringPdf = 0f;
            var Ld = Spectrum.Zero;
            var Li = light.Sample_Li(it, uLight, out var wi, out var lightPdf, out var visibility);
            if (lightPdf > 0f && !Li.IsBlack())
            {
                Spectrum f = Spectrum.Zero;
                if (it.IsSurfaceInteraction)
                {
                    if (!(it is SurfaceInteraction si))
                    {
                        throw new InvalidOperationException("Expecting a Surface Interaction for 'it'.");
                    }

                    f = si.BSDF.F(si.Wo, wi, bsdfFlags) * System.MathF.Abs(wi % si.ShadingGeometry.N);
                    scatteringPdf = si.BSDF.Pdf(si.Wo, wi, bsdfFlags);
                }

                if (!f.IsBlack())
                {
                    if (handleMedia)
                    {
                        Li *= visibility.Tr(scene, sampler);
                    }
                    else
                    {
                        if (!visibility.Unoccluded(scene))
                        {
                            Li = Spectrum.Zero;
                        }
                    }

                    if (!Li.IsBlack())
                    {
                        if (light.IsDeltaLight())
                        {
                            Ld += f * Li / lightPdf;
                        }
                        else
                        {
                            var weight = PowerHeuristic(1, lightPdf, 1, scatteringPdf);
                            Ld += f * Li * (weight / lightPdf);
                        }
                    }
                }
            }

            if (!light.IsDeltaLight())
            {
                Spectrum f = Spectrum.Zero;
                var sampledSpecular = false;
                if (it.IsSurfaceInteraction)
                {
                    if (!(it is SurfaceInteraction si))
                    {
                        throw new InvalidOperationException("Expecting a Surface Interaction for 'it'.");
                    }


                    f = si.BSDF.Sample_F(si.Wo, out wi, uScattering, out scatteringPdf, bsdfFlags, out var sampledType);
                    f *= System.MathF.Abs(wi % si.ShadingGeometry.N);
                    sampledSpecular = (sampledType & BxDFType.Specular) != BxDFType.None;
                }

                if (!f.IsBlack() && scatteringPdf > 0f)
                {
                    var weight = 1f;
                    if (!sampledSpecular)
                    {
                        lightPdf = light.Pdf_Li(it, wi);
                        if (lightPdf == 0f)
                        {
                            return Ld;
                        }

                        weight = PowerHeuristic(1, scatteringPdf, 1, lightPdf);
                    }

                    var lightIsect = new SurfaceInteraction();
                    var ray = new RayDifferential(it.SpawnRay(wi));
                    var tr = Spectrum.One;
                    var foundSurfaceInteraction = handleMedia
                        ? scene.IntersectTr(ray, sampler, ref lightIsect, out tr)
                        : scene.Intersect(ray, ref lightIsect);

                    if (foundSurfaceInteraction)
                    {
                        if (ReferenceEquals(lightIsect.Primitive.AreaLight, light))
                        {
                            Li = lightIsect.Le(-wi);
                        }
                    }
                    else
                    {
                        Li = light.Le(in ray);
                    }

                    if (!Li.IsBlack())
                    {
                        Ld += f * Li * tr * weight / scatteringPdf;
                    }
                }
            }

            return Ld;
        }

        [Pure]
        public static Distribution1D ComputeLightPowerDistribution(this IScene scene)
        {
            if (scene.Lights.Length == 0) return null;
            var lightPower = scene.Lights.Select(light => light.Power().YComponent()).ToList();
            return new Distribution1D(lightPower.ToArray(), lightPower.Count);
        }
    }
}