using Octans.Reflection;
using Octans.Reflection.Microfacet;

namespace Octans.Material
{
    public sealed class PlasticMaterial : IMaterial
    {
        public PlasticMaterial(ITexture2<Spectrum> kd,
                               ITexture2<Spectrum> ks,
                               ITexture2<float> roughness,
                               ITexture2<float> bumpMap,
                               bool remapRoughness)
        {
            Kd = kd;
            Ks = ks;
            Roughness = roughness;
            BumpMap = bumpMap;
            RemapRoughness = remapRoughness;
        }

        public ITexture2<Spectrum> Kd { get; }
        public ITexture2<Spectrum> Ks { get; }
        public ITexture2<float> Roughness { get; }
        public ITexture2<float> BumpMap { get; }
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
            if (ks.IsBlack())
            {
                return;
            }

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