using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans.Reflection
{
    public class BSDF
    {
        private const int MaxBxDFs = 8;
        private readonly IBxDF[] _bxdf = new IBxDF[MaxBxDFs];
        private readonly Normal ng, ns;
        private readonly Vector ss, ts;
        private int _nBxDFs;

        // TODO: Object pool and initialize method may improve perf.
        public BSDF(in SurfaceInteraction si, float eta = 1f)
        {
            // TODO: Set shading info from si
            Eta = eta;
            
        }

        public float Eta { get; }

        public void Add(in IBxDF bxdf)
        {
            Debug.Assert(_nBxDFs < MaxBxDFs, $"BSDF can only hold {MaxBxDFs} BxDFs.");
            _bxdf[_nBxDFs++] = bxdf;
        }

        [Pure]
        public int NumberOfComponents(BxDFType flags = BxDFType.All)
        {
            var count = 0;
            for (var i = 0; i < _nBxDFs; i++)
            {
                if (_bxdf[i].IsType(flags))
                {
                    ++count;
                }
            }

            return count;
        }

        [Pure]
        public Spectrum F(in Vector woW, in Vector wiW, BxDFType flags)
        {
            var wi = WorldToLocal(in wiW);
            var wo = WorldToLocal(in woW);
            var reflect = wiW % ng * (woW % ng) > 0f;
            var f = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var current = _bxdf[i];
                if (current.IsType(flags) &&
                    (reflect && current.IsType(BxDFType.Reflection) ||
                     !reflect && current.IsType(BxDFType.Transmission)))
                {
                    f += current.F(in wo, in wi);
                }
            }

            return f;
        }

        [Pure]
        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u, BxDFType flags = BxDFType.All)
        {
            var ret = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var current = _bxdf[i];
                if (current.IsType(flags))
                {
                    ret += current.Rho(in wo, nSamples, in u);
                }
            }

            return ret;
        }

        [Pure]
        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2, BxDFType flags = BxDFType.All)
        {
            var ret = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var current = _bxdf[i];
                if (current.IsType(flags))
                {
                    ret += current.Rho(nSamples, in u1, in u2);
                }
            }

            return ret;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector WorldToLocal(in Vector v) => new Vector(v % ss, v % ts, v % ns);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector LocalToWorld(in Vector v) =>
            new Vector(
                ss.X * v.X + ts.X * v.Y + ns.X * v.Z,
                ss.Y * v.X + ts.Y * v.Y + ns.Y * v.Z,
                ss.Z * v.X + ts.Z * v.Y + ns.Z * v.Z);
    }
}