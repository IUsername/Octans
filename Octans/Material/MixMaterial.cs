using Octans.Memory;
using Octans.Reflection;

namespace Octans.Material
{
    public sealed class MixMaterial : IMaterial
    {
        public IMaterial M1 { get; }
        public IMaterial M2 { get; }
        public Texture2<Spectrum> Scale { get; }

        public MixMaterial(IMaterial m1, IMaterial m2, Texture2<Spectrum> scale)
        {
            M1 = m1;
            M2 = m2;
            Scale = scale;
        }

        public void ComputeScatteringFunctions(SurfaceInteraction si, IObjectArena arena, TransportMode mode, bool allowMultipleLobes)
        {
            var s1 = Scale.Evaluate(in si).Clamp();
            var s2 = (Spectrum.One - s1).Clamp();
            M1.ComputeScatteringFunctions(si, arena, mode, allowMultipleLobes);

            var si2 = arena.Create<SurfaceInteraction>().Initialize(in si);
            M2.ComputeScatteringFunctions(si2, arena, mode, allowMultipleLobes);

            var n1 = si.BSDF.NumberOfComponents();
            var n2 = si2.BSDF.NumberOfComponents();

            for (var i = 0; i < n1; ++i)
            {
                var collection = (IBxDFCollection) si.BSDF;
                var bxdf = arena.Create<ScaledBxDF>().Initialize(collection[i], s1);
                collection.Set(bxdf, i);
            }

            for (var i = 0; i < n2; ++i)
            {
                var collection = (IBxDFCollection) si2.BSDF;
                var bxdf = arena.Create<ScaledBxDF>().Initialize(collection[i], s2);
                collection.Set(bxdf, i);
            }
        }
    }
}