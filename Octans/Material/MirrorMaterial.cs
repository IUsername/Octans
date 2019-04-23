using Octans.Reflection;

namespace Octans.Material
{
    public class MirrorMaterial : IMaterial
    {
        private readonly ITexture2<float> _bumpMap;
        private readonly ITexture2<Spectrum> _kr;

        public MirrorMaterial(ITexture2<Spectrum> r, ITexture2<float> bumpMap)
        {
            _kr = r;
            _bumpMap = bumpMap;
        }

        public void ComputeScatteringFunctions(SurfaceInteraction si,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               bool allowMultipleLobes)
        {
            _bumpMap?.Bump(si);
            si.BSDF.Initialize(si);
            var R = _kr.Evaluate(si).Clamp();
            if (!R.IsBlack())
            {
                si.BSDF.Add(arena.Create<SpecularReflection>().Initialize(R, arena.Create<FresnelNoOp>()));
            }
        }
    }
}