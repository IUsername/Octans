using Octans.Reflection;
using Octans.Reflection.Microfacet;

namespace Octans.Material
{
    public class MetalMaterial : IMaterial
    {
        private readonly ITexture2<float> _bumpMap;
        private readonly ITexture2<Spectrum> _eta;
        private readonly ITexture2<Spectrum> _k;
        private readonly bool _remapRoughness;
        private readonly ITexture2<float> _roughness;
        private readonly ITexture2<float> _uRoughness;
        private readonly ITexture2<float> _vRoughness;

        public MetalMaterial(ITexture2<Spectrum> eta,
                             ITexture2<Spectrum> k,
                             ITexture2<float> roughness,
                             ITexture2<float> uRoughness,
                             ITexture2<float> vRoughness,
                             ITexture2<float> bumpMap,
                             bool remapRoughness)
        {
            _eta = eta;
            _k = k;
            _roughness = roughness;
            _uRoughness = uRoughness;
            _vRoughness = vRoughness;
            _bumpMap = bumpMap;
            _remapRoughness = remapRoughness;
        }

        public void ComputeScatteringFunctions(SurfaceInteraction si,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               bool allowMultipleLobes)
        {
            _bumpMap?.Bump(si);

            si.BSDF.Initialize(si);

            var uRough = _uRoughness?.Evaluate(si) ?? _roughness.Evaluate(si);
            var vRough = _vRoughness?.Evaluate(si) ?? _roughness.Evaluate(si);

            if (_remapRoughness)
            {
                uRough = TrowbridgeReitzDistribution.RoughnessToAlpha(uRough);
                vRough = TrowbridgeReitzDistribution.RoughnessToAlpha(vRough);
            }

            var fr = arena.Create<FresnelConductor>().Initialize(Spectrum.One, _eta.Evaluate(si), _k.Evaluate(si));

            var dist = arena.Create<TrowbridgeReitzDistribution>().Initialize(uRough, vRough);
            si.BSDF.Add(arena.Create<MicrofacetReflection>().Initialize(Spectrum.One, dist, fr));
        }
    }
}