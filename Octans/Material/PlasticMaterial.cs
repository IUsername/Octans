using Octans.Memory;
using Octans.Reflection;
using Octans.Reflection.Microfacet;

namespace Octans.Material
{
    public sealed class PlasticMaterial : IMaterial
    {
        public PlasticMaterial(Texture2<Spectrum> kd,
                               Texture2<Spectrum> ks,
                               Texture2<float> roughness,
                               Texture2<float> bumpMap,
                               bool remapRoughness)
        {
            Kd = kd;
            Ks = ks;
            Roughness = roughness;
            BumpMap = bumpMap;
            RemapRoughness = remapRoughness;
        }

        public Texture2<Spectrum> Kd { get; }
        public Texture2<Spectrum> Ks { get; }
        public Texture2<float> Roughness { get; }
        public Texture2<float> BumpMap { get; }
        public bool RemapRoughness { get; }

        public void ComputeScatteringFunctions(SurfaceInteraction si,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               bool allowMultipleLobes)
        {
            BumpMap?.Bump(si);
            var bsdf = si.BSDF.Initialize(in si);
            var kd = Kd.Evaluate(in si).Clamp();
            if (!kd.IsBlack())
            {
                bsdf.Add(arena.Create<LambertianReflection>().Initialize(in kd));
            }

            var ks = Ks.Evaluate(in si).Clamp();
            if (!ks.IsBlack())
            {
                var fresnel = arena.Create<FresnelDielectric>().Initialize(1f, 1.5f);
                var rough = Roughness.Evaluate(in si);
                if (RemapRoughness)
                {
                    rough = TrowbridgeReitzDistribution.RoughnessToAlpha(rough);
                }

                var distribution = arena.Create<TrowbridgeReitzDistribution>().Initialize(rough, rough);
                var specular = arena.Create<MicrofacetReflection>().Initialize(ks, distribution, fresnel);
                bsdf.Add(specular);
            }
        }
    }
}