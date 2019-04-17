using Octans.Memory;
using Octans.Reflection;
using static Octans.MathF;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Octans.Material
{
    public class MatteMaterial : IMaterial
    {
        public MatteMaterial(ITexture2<Spectrum> kd, ITexture2<float> sigma, ITexture2<float> bumpMap)
        {
            Kd = kd;
            Sigma = sigma;
            BumpMap = bumpMap;
        }

        public ITexture2<Spectrum> Kd { get; }
        public ITexture2<float> Sigma { get; }
        public ITexture2<float> BumpMap { get; }

        public void ComputeScatteringFunctions(SurfaceInteraction si,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               bool allowMultipleLobes)
        {
            BumpMap?.Bump(si);

            var bsdf = si.BSDF.Initialize(in si);
            var r = Kd.Evaluate(in si).Clamp();
            if (r.IsBlack())
            {
                return;
            }

            var sig = Clamp(0, 90, Sigma.Evaluate(in si));
            if (sig == 0f)
            {
                bsdf.Add(arena.Create<LambertianReflection>().Initialize(in r));
            }
            else
            {
                bsdf.Add(arena.Create<OrenNayar>().Initialize(in r, sig));
            }
        }
    }
}