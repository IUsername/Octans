using System;
using Octans.Light;

namespace Octans.Shading
{
    public class ShadingContext
    {
        private readonly IFresnelFunction _ff;
        private readonly IGeometricShadow _gsf;
        private readonly INormalDistribution _ndf;

        public ShadingContext(int minDepth,
                              int maxDepth,
                              INormalDistribution ndf,
                              IGeometricShadow gsf,
                              in IFresnelFunction ff)
        {
            MinDepth = minDepth;
            MaxDepth = maxDepth;
            _ndf = ndf;
            _gsf = gsf;
            _ff = ff;
        }

        public int MinDepth { get; }

        public int MaxDepth { get; }

        public static bool IsShadowed(World w, in Point p, in Point lightPoint, in Normal normal)
        {
            var v = lightPoint - p;
            var nDotN = v % normal;
            if (nDotN <= 0f)
            {
                return false;
            }

            var direction = v.Normalize();
            var r = new Ray(p, direction);
            // TODO: Optimized shadow hit test
            var xs = w.Intersect(in r);
            var h = xs.Hit(true);
            xs.Return();
            return h.HasValue && h.Value.T < v.Magnitude();
        }

        public Color ColorAt(World world, in Ray ray, ISampler sampler)
        {
            var currentRay = ray;
            var throughPut = Colors.White;
            var color = Colors.Black;
            //var depthFactor = 1 / (MaxDepth);

            for (var depth = 0; depth < MaxDepth; depth++)
            {
                var xs = world.Intersect(in currentRay);
                var hit = xs.Hit();
                xs.Return();
                if (!hit.HasValue)
                {
                    break;
                }

                var info = new IntersectionInfo(hit.Value, currentRay);
                var material = info.Geometry.Material;
                var ambient = material.Ambient;
                var surfaceColor = material.Texture.ShapeColor(info.Geometry, info.OverPoint);

                // Illumination Equation
                // I = k_a*I_a + I_i(k_d(L%N) + k_s(V%R)^n) + k_t*I_t + k_r*I_r;
                // Ambient + Direct Diffuse + Direct Specular + Indirect (Specular & Diffuse)

                color += ambient * surfaceColor * throughPut;
                // TODO: This is a hack/assumption to jump out for quick shading of environment map.
                if (ambient >= 1f)
                {
                    break;
                }

                var probabilityOfReflection = MathFunction.MixF(1f, Schlick(in info), material.Transparency);
                if (sampler.Random() <= probabilityOfReflection)
                {
                    //   throughPut *= 1f - material.Transparency;
                    // Direct + Indirect

                    var directProbability = MathFunction.MixF(0.5f,
                                                              MathFunction.MixF(0.0f, 0.5f, material.Roughness),
                                                              material.Metallic);

                    if (sampler.Random() <= directProbability)
                    {
                        var direct = DirectLighting(world, info, sampler);
                        color += direct * throughPut; //* (1 / directProbability);
                        break;
                    }

                    throughPut *= 1f / (1f - directProbability);

                    // Indirect lighting
                    (currentRay, throughPut) = IndirectLighting(sampler, info, in throughPut);
                }
                else
                {
                    //  throughPut *= 1f / probabilityOfReflection;

                    // Transmission
                    (currentRay, throughPut) = Transmitted(in info, in throughPut, sampler);
                }

                if (throughPut == Colors.Black)
                {
                    break;
                }


                if (depth < MinDepth)
                {
                    continue;
                }

                // Russian roulette
                var maxChannel = MathF.Max(throughPut.Red, MathF.Max(throughPut.Green, throughPut.Blue));
                var continueProbability = MathFunction.ClampF(0f, 1f, maxChannel);
                if (sampler.Random() > continueProbability)
                {
                    break;
                }

                throughPut *= 1f / continueProbability;

                //var stopProbability = MathF.Min(1f, depthFactor * (depth));
                //if (sampler.Random() <= stopProbability)
                //{
                //    break;
                //}

                //throughPut *= 1f / (1f - stopProbability);
            }

            return color;
        }

        private (Ray bounce, Color throughPut) IndirectLighting(ISampler sampler,
                                                                IntersectionInfo info,
                                                                in Color throughPut)
        {
            var localFrame = new LocalFrame(info.Normal);
            var (e0, e1) = sampler.NextUV();
            while (true)
            {
                var (wi, f) = _ndf.Sample(in info, in localFrame, e0, e1);
                if (wi.Z > 0f)
                {
                    var direction = localFrame.ToWorld(in wi);
                    var currentRay = new Ray(info.OverPoint, direction);
                    return (currentRay, throughPut * f);
                }

                if (wi.Z == 0)
                {
                    // Total internal reflection.
                    return (new Ray(), Colors.Black);
                }

                (e0, e1) = sampler.NextUV();
            }
        }

        private Color DirectLighting(World world, IntersectionInfo info, ISampler sampler)
        {
            var surface = Colors.Black;
            // Direct lighting
            for (var i = 0; i < world.Lights.Count; i++)
            {
                var light = world.Lights[i];
                // ReSharper disable InconsistentNaming
                var (intensity, sampled) = IntensityAt(world, info.OverPoint, info.Normal, light, sampler);


                if (!(intensity > 0f))
                {
                    continue;
                }

                var si = new ShadingInfo(intensity, in light, in info, in sampled);
                var specularIntensity = 0f;
                var denominator = 4f * si.NdotL * si.NdotV;
                if (denominator > 0.001f)
                {
                    var D = 1f;
                    D *= _ndf.Factor(in si);

                    var G = 1f;
                    G *= _gsf.Factor(in si);

                    var F = 1f;
                    F *= _ff.Factor(in si);

                    specularIntensity = D * F * G / denominator;
                }

                // TODO: Unify. Diffuse color already includes diffuse intensity/attenuation at the moment.
                var k_d = si.DiffuseColor;
                var k_s = si.SpecularColor * specularIntensity;

                var c = k_d + k_s;
                // Lambert's cosine law.
                surface += si.LightIntensity * c * si.NdotL;
                // ReSharper restore InconsistentNaming
            }

            return surface;
        }

        public (Ray ray, Color throughPut) Transmitted(in IntersectionInfo info, in Color throughput, ISampler sampler)
        {
            if (info.Geometry.Material.Transparency <= 0f)
            {
                return (new Ray(), Colors.Black);
            }

            if (!info.IsInside)
            {
                var localFrame = new LocalFrame(info.Normal);
                var (e0, e1) = sampler.NextUV();
                //while (true)
                //{
                var (wi, f) = _ndf.SampleTransmission(in info, in localFrame, e0, e1);
                //if (wi.Z != 0f)
                //{
                var diffuseDir = localFrame.ToWorld(in wi);
                var currentRay = new Ray(info.UnderPoint, diffuseDir);
                // TODO: factor is ignored because it is not correct.
                //return (currentRay, throughput);
                return (currentRay, throughput * f);
                //}

                //if (wi.Z == 0)
                //{
                // Total internal reflection.
                //return (new Ray(), Colors.Black);
                //}

                //(e0, e1) = sampler.NextUV();
                //}
            }

            var nRatio = info.N1 / info.N2;
            var cosI = info.Eye % info.Normal;
            var sin2T = nRatio * nRatio * (1f - cosI * cosI);
            if (sin2T > 1f)
            {
                // Total internal reflection.
                return (new Ray(), Colors.Black);
            }

            var cosT = MathF.Sqrt(1f - sin2T);
            var direction = (Vector) info.Normal * (nRatio * cosI - cosT) - info.Eye * nRatio;


            return (new Ray(info.UnderPoint, direction), throughput);
        }


        public static float Schlick(in IntersectionInfo info)
        {
            // Cosine of angle between eye and normal vectors.
            var cos = info.Eye % info.Normal;

            // Total internal reflections can only occur when N1 > N2.
            if (info.N1 > info.N2)
            {
                var n = info.N1 / info.N2;
                var sin2T = n * n * (1f - cos * cos);
                if (sin2T > 1f)
                {
                    return 1.0f;
                }

                // Cosine of theta_t
                var cosT = MathF.Sqrt(1f - sin2T);
                cos = cosT;
            }

            var r0 = (info.N1 - info.N2) / (info.N1 + info.N2);
            r0 *= r0;
            // Probability of reflection
            var rProb = r0 + (1f - r0) * MathF.Pow(1 - cos, 5);
            return rProb;
        }

        public static (float intensity, Point sampledPoint) IntensityAt(
            World world,
            in Point point,
            in Normal normal,
            ILight light,
            ISampler sampler)
        {
            switch (light)
            {
                case PointLight _:
                    return (IsShadowed(world, in point, light.Position, in normal) ? 0.0f : 1.0f, light.Position);
                case AreaLight area:
                {
                    var (lu, lv) = sampler.NextUV();
                    var lPoint = area.GetPoint(lu, lv);
                    return (!IsShadowed(world, in point, in lPoint, in normal) ? 1f : 0f, lPoint);


                    //    var total = 0.0f;
                    //for (var v = 0; v < area.VSteps; v++)
                    //{
                    //    for (var u = 0; u < area.USteps; u++)
                    //    {
                    //        if (!IsShadowed(world, in point, area.UVPoint(u, v)))
                    //        {
                    //            total += 1.0f;
                    //        }
                    //    }
                    //}

                    //return total / area.Samples;
                }
                default:
                    return (0f, new Point(0, 0, 0));
            }
        }
    }
}