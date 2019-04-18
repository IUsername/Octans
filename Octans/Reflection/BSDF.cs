using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Octans.Reflection
{
    public class BSDF : IBxDFCollection
    {
        private const int MaxBxDFs = 8;
        private readonly IBxDF[] _bxdf = new IBxDF[MaxBxDFs];
        private Normal _ng, _ns;
        private Vector _ss, _ts;
        private int _nBxDFs;

        public BSDF Initialize(in SurfaceInteraction si, float eta = 1f)
        {
            _ns = si.ShadingGeometry.N;
            _ng = si.N;
            _ss = si.ShadingGeometry.Dpdu.Normalize();
            _ts = Vector.Cross(_ns, _ss);
            Eta = eta;
            _nBxDFs = 0;
            //si.BSDF = this;
            return this;
        }

        IBxDF IBxDFCollection.this[int index] => (index < _nBxDFs) ? _bxdf[index] : null;

        public float Eta { get; private set; }

        public void Add(in IBxDF bxdf)
        {
            Debug.Assert(_nBxDFs < MaxBxDFs, $"BSDF can only hold {MaxBxDFs} BxDFs.");
            _bxdf[_nBxDFs++] = bxdf;
        }

        void IBxDFCollection.Set(in IBxDF bxdf, int index)
        {
            if (index < _nBxDFs)
            {
                _bxdf[index] = bxdf;
            }
            else
            {
                throw new IndexOutOfRangeException("The index must have been previously allocated before using the Add method.");
            }
        }

        [Pure]
        public int NumberOfComponents(BxDFType flags = BxDFType.All)
        {
            var count = 0;
            for (var i = 0; i < _nBxDFs; i++)
            {
                if (_bxdf[i].IsFlagged(flags))
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
            var reflect = wiW % _ng * (woW % _ng) > 0f;
            var f = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var b = _bxdf[i];
                if (b.IsFlagged(flags) &&
                    (reflect && b.IsFlagged(BxDFType.Reflection) ||
                     !reflect && b.IsFlagged(BxDFType.Transmission)))
                {
                    f += b.F(in wo, in wi);
                }
            }

            return f;
        }

        [Pure]
        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u, BxDFType flags = BxDFType.All)
        {
            var ret = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var current = _bxdf[i];
                if (current.IsFlagged(flags))
                {
                    ret += current.Rho(in wo, nSamples, in u);
                }
            }

            return ret;
        }

        [Pure]
        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2, BxDFType flags = BxDFType.All)
        {
            var ret = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var current = _bxdf[i];
                if (current.IsFlagged(flags))
                {
                    ret += current.Rho(nSamples, in u1, in u2);
                }
            }

            return ret;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector WorldToLocal(in Vector v) => new Vector(v % _ss, v % _ts, v % _ns);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector LocalToWorld(in Vector v) =>
            new Vector(
                _ss.X * v.X + _ts.X * v.Y + _ns.X * v.Z,
                _ss.Y * v.X + _ts.Y * v.Y + _ns.Y * v.Z,
                _ss.Z * v.X + _ts.Z * v.Y + _ns.Z * v.Z);
    }
}