using System;
using System.Data;
using Octans.Light;

namespace Octans.Shading
{
    public class ShadingContext
    {
        private readonly IFresnelFunction _ff;
        private readonly IGeometricShadow _gsf;
        private readonly INormalDistribution _ndf;

        //private static readonly Sequence _uns = Sequence.LargeRandomZeroOne();
      

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
            var surface = Colors.Black;
            float f0 = 0f;
            Vector wi = new Vector(0,0,0);
            ShadingInfo si = default;
            bool set = false;
            var f = Colors.Black;

            foreach (var light in world.Lights)
            {
                var intensity = IntensityAt(world, info.OverPoint, light);
                si = new ShadingInfo(intensity, in light, in info);
                set = true;
                var specular = 0f;
                var denominator = 4f * si.NdotL * si.NdotV;
                if (denominator > 0.0005f)
                {
                    // ReSharper disable InconsistentNaming
                    var D = 1f;
                    D *= _ndf.Factor(in si);

                    var G = 1f;
                    G *= _gsf.Factor(in si);

                    var F = 1f;
                    F *= _ff.Factor(in si);

                    specular = D * F * G / denominator;
                    // ReSharper restore InconsistentNaming
                }

                var ambient = si.DiffuseColor * info.Geometry.Material.Ambient * light.Intensity;
                var direct = (specular * si.SpecularColor + si.DiffuseColor) * si.NdotL * si.AttenuationColor;
                surface += direct + ambient;
              // surface += ambient;

                f0 = si.F0;

                
            }

            var next = remaining - 1;
            if (next > 1)
            {
                //if (si.Roughness == 0f)
                //{
                //   surface += ReflectedColor(world, info, remaining);
                //}
                //else
                //{
             
                if (set)
                    {
                    var localFrame = new LocalFrame(si.NormalDirection);

                    var reflected = Colors.Black;
                        int count = 0;
                        for (int i = 0; i <16; i++)
                        {
                            var e0 = (float) MersenneTwister.Randoms.FastestDouble.NextDouble();
                            var e1 = (float) MersenneTwister.Randoms.FastestDouble.NextDouble();
                            (wi, f) = _ndf.Sample(in si, in localFrame,e0,e1);
                            count++;

                            var wWi = localFrame.ToWorld(in wi);

                        var reflectedRay = new Ray(info.OverPoint, wWi);
                            var color = ColorAt(world, in reflectedRay, next);
                            reflected +=  (color * f);
                           

                        }

                        surface += reflected / count;
                    }
                //}


            }

            return surface;



            //var reflected = ReflectedColor(world, info, remaining);
            //var refracted = RefractedColor(world, info, remaining);

            //var material = info.Geometry.Material;
            //if (material.Reflective > 0f && material.Transparency > 0f)
            //{
            //    var reflectance = Schlick(in info);
            //    return surface + reflected * reflectance + refracted * (1f - reflectance);
            //}

            //return surface + reflected + refracted;
        }

        public Color ColorAt(World world, in Ray ray, int remaining = 5)
        {
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

        public Color ReflectedColor(World world, in IntersectionInfo info, int remaining = 5)
        {
            if (remaining < 1)
            {
                return Colors.Black;
            }

            var roughness = info.Geometry.Material.Roughness;
            var reflective = 1f - MathF.Pow(roughness, 0.14f);

            //var reflective = info.Geometry.Material.Reflective;
            if (reflective <= 0f)
            {
                return Colors.Black;
            }

            var reflectedRay = new Ray(info.OverPoint, info.Reflect);
            var color = ColorAt(world, in reflectedRay, --remaining);
            return color * reflective;
        }

        public Color RefractedColor(World world, in IntersectionInfo info, int remaining)
        {
            if (remaining < 1)
            {
                return Colors.Black;
            }

            if (info.Geometry.Material.Transparency <= 0f)
            {
                return Colors.Black;
            }

            var nRatio = info.N1 / info.N2;
            var cosI = info.Eye % info.Normal;
            var sin2T = nRatio * nRatio * (1f - cosI * cosI);
            if (sin2T > 1f)
            {
                // Total internal reflection.
                return Colors.Black;
            }

            var cosT = MathF.Sqrt(1f - sin2T);
            var direction = info.Normal * (nRatio * cosI - cosT) - info.Eye * nRatio;
            var refractedRay = new Ray(info.UnderPoint, direction);
            return ColorAt(world, in refractedRay, --remaining) * info.Geometry.Material.Transparency;
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

            // r0 == F0 - equivalent refractive index
            var r0 = (info.N1 - info.N2) / (info.N1 + info.N2);
            r0 *= r0;
            return r0 + (1f - r0) * MathF.Pow(1 - cos, 5);
        }

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