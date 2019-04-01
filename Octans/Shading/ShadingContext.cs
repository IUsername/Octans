using System.Threading;
using Octans.Light;

namespace Octans.Shading
{
    public class ShadingContext
    {
        private readonly IFresnelFunction _ff;
        private readonly IGeometricShadow _gsf;
        private readonly INormalDistribution _ndf;

        private readonly ThreadLocal<long> _index =
            new ThreadLocal<long>(() => Thread.CurrentThread.ManagedThreadId * 10);

        public ShadingContext(INormalDistribution ndf, IGeometricShadow gsf, in IFresnelFunction ff)
        {
            _ndf = ndf;
            _gsf = gsf;
            _ff = ff;
        }

        public static bool IsShadowed(World w, in Point p, in Point lightPoint)
        {
            var v = lightPoint - p;
            var distance = v.Magnitude();
            var direction = v.Normalize();
            var r = new Ray(p, direction);
            var xs = w.Intersect(in r);
            var h = xs.Hit(true);
            xs.Return();
            return h.HasValue && h.Value.T < distance;
        }

        public Color HitColor(World world, in IntersectionInfo info, int remaining = 5)
        {
            // TODO: Forward propagate the quasi-random sampling instead of thread local tracking.
            var ambient = info.Geometry.Material.Ambient;
            var surfaceColor = info.Geometry.Material.Texture.ShapeColor(info.Geometry, info.OverPoint);

            // Illumination Equation
            // I = k_a*I_a + I_i(k_d(L%N) + k_s(V%R)^n) + k_t*I_t + k_r*I_r;
            // Ambient + Direct Diffuse + Direct Specular + Indirect (Specular & Diffuse)

            var surface = ambient * surfaceColor;
            // TODO: This is a hack/assumption to jump out for quick shading of environment map.
            if (ambient >= 1f)
            {
                return surface;
            }

            // Direct lighting
            foreach (var light in world.Lights)
            {
                // ReSharper disable InconsistentNaming
                var intensity = IntensityAt(world, info.OverPoint, light);
                var specularIntensity = 0f;
                var k_d = Colors.Black;
                var k_s = Colors.Black;
                var si = new ShadingInfo(intensity, in light, in info);
                if (intensity > 0f)
                {
                    var denominator = 4f * si.NdotL * si.NdotV;
                    if (denominator > 0f)
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
                    k_d = si.DiffuseColor;
                    k_s = si.SpecularColor * specularIntensity;
                }
                // ReSharper restore InconsistentNaming

                var c = k_d + k_s;
                // Lambert's cosine law.
                surface += si.LightIntensity * c * si.NdotL;
            }

            if (remaining-- < 1)
            {
                return surface;
            }

            // Indirect lighting
            var localFrame = new LocalFrame(info.Normal);
            var indirect = Colors.Black;

            // TODO: Make parameter for ray count.
            var rayCount = 3;
            var captured = 0;

            var di = _index.Value;
            while (captured < rayCount)
            {
                //var e0 = (float)Randoms.WellBalanced.NextDouble();
                //var e1 = (float)Randoms.WellBalanced.NextDouble();
                var (e0, e1) = QuasiRandom.Next(di++);
                var (wi, f) = _ndf.Sample(in info, in localFrame, e0, e1);
                if (!(wi.Z > 0f))
                {
                    continue;
                }

                var direction = localFrame.ToWorld(in wi);
                var reflectedRay = new Ray(info.OverPoint, direction);
                var color = ColorAt(world, in reflectedRay, remaining);
                indirect += color * f;
                captured++;
            }

            _index.Value = di;
            indirect /= captured;
            surface += indirect;
            return surface;
        }

        public Color ColorAt(World world, in Ray ray, int remaining = 5)
        {
            //// TODO: Russian roulette path termination
            //// Need to count up ray depth
            //var rrFactor = 1f;
            //if (depth >= 5)
            //{
            //    const float stopProbability = MathF.Min(1f, 0.0625f * depth);
            //    if (rnd <= stopProbability)
            //    {
            //        return;
            //    }

            //    rrFactor = 1f / (1f - stopProbability);
            //}

            var xs = world.Intersect(in ray);
            var hit = xs.Hit();
            xs.Return();
            if (!hit.HasValue)
            {
                return Colors.Black;
            }

            var info = new IntersectionInfo(hit.Value, ray);
            return HitColor(world, info, remaining);
        }

        //public Color ReflectedColor(World world, in IntersectionInfo info, int remaining = 5)
        //{
        //    if (remaining < 1)
        //    {
        //        return Colors.Black;
        //    }

        //    var roughness = info.Geometry.Material.Roughness;
        //    var reflective = 1f - MathF.Pow(roughness, 0.14f);

        //    //var reflective = info.Geometry.Material.Reflective;
        //    if (reflective <= 0f)
        //    {
        //        return Colors.Black;
        //    }

        //    var reflectedRay = new Ray(info.OverPoint, info.Reflect);
        //    var color = ColorAt(world, in reflectedRay, --remaining);
        //    return color * reflective;
        //}

        //public Color RefractedColor(World world, in IntersectionInfo info, int remaining)
        //{
        //    if (remaining < 1)
        //    {
        //        return Colors.Black;
        //    }

        //    if (info.Geometry.Material.Transparency <= 0f)
        //    {
        //        return Colors.Black;
        //    }

        //    var nRatio = info.N1 / info.N2;
        //    var cosI = info.Eye % info.Normal;
        //    var sin2T = nRatio * nRatio * (1f - cosI * cosI);
        //    if (sin2T > 1f)
        //    {
        //        // Total internal reflection.
        //        return Colors.Black;
        //    }

        //    var cosT = MathF.Sqrt(1f - sin2T);
        //    var direction = info.Normal * (nRatio * cosI - cosT) - info.Eye * nRatio;
        //    var refractedRay = new Ray(info.UnderPoint, direction);
        //    return ColorAt(world, in refractedRay, --remaining) * info.Geometry.Material.Transparency;
        //}

        //public static float Schlick(in IntersectionInfo info)
        //{
        //    // Cosine of angle between eye and normal vectors.
        //    var cos = info.Eye % info.Normal;

        //    // Total internal reflections can only occur when N1 > N2.
        //    if (info.N1 > info.N2)
        //    {
        //        var n = info.N1 / info.N2;
        //        var sin2T = n * n * (1f - cos * cos);
        //        if (sin2T > 1f)
        //        {
        //            return 1.0f;
        //        }

        //        // Cosine of theta_t
        //        var cosT = MathF.Sqrt(1f - sin2T);
        //        cos = cosT;
        //    }

        //    // r0 == F0 - equivalent refractive index
        //    var r0 = (info.N1 - info.N2) / (info.N1 + info.N2);
        //    r0 *= r0;
        //    return r0 + (1f - r0) * MathF.Pow(1 - cos, 5);
        //}

        public static float IntensityAt(World world, in Point point, ILight light)
        {
            switch (light)
            {
                case PointLight _:
                    return IsShadowed(world, in point, light.Position) ? 0.0f : 1.0f;
                case AreaLight area:
                {
                    var total = 0.0f;
                    for (var v = 0; v < area.VSteps; v++)
                    {
                        for (var u = 0; u < area.USteps; u++)
                        {
                            if (!IsShadowed(world, in point, area.UVPoint(u, v)))
                            {
                                total += 1.0f;
                            }
                        }
                    }

                    return total / area.Samples;
                }
                default:
                    return 0f;
            }
        }
    }
}