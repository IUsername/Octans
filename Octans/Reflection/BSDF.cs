using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class BSDF : IBxDFCollection
    {
        private const int MaxBxDFs = 8;
        private readonly IBxDF[] _bxdf = new IBxDF[MaxBxDFs];
        private int _nBxDFs;
        private Normal _ng, _ns;
        private Vector _ss, _ts;

        public float Eta { get; private set; }

        IBxDF IBxDFCollection.this[int index] => index < _nBxDFs ? _bxdf[index] : null;

        void IBxDFCollection.Set(in IBxDF bxdf, int index)
        {
            if (index < _nBxDFs)
            {
                _bxdf[index] = bxdf;
            }
            else
            {
                throw new IndexOutOfRangeException(
                    "The index must have been previously allocated before using the Add method.");
            }
        }

        public BSDF Initialize(in SurfaceInteraction si, float eta = 1f)
        {
            _ns = si.ShadingGeometry.N;
            _ng = si.N;
            _ss = si.ShadingGeometry.Dpdu.Normalize();
            _ts = Vector.Cross(_ns, _ss);
            Eta = eta;
            _nBxDFs = si.BSDF._nBxDFs;
            for (var i = 0; i < _nBxDFs; i++)
            {
                _bxdf[i] = si.BSDF._bxdf[i];
            }

            // si.BSDF = this;
            return this;
        }

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
                if (_bxdf[i].AnyFlag(flags))
                {
                    ++count;
                }
            }

            return count;
        }

        [Pure]
        public Spectrum F(in Vector woW, in Vector wiW, BxDFType flags = BxDFType.All)
        {
            var wi = WorldToLocal(in wiW);
            var wo = WorldToLocal(in woW);
            if (wo.Z == 0f)
            {
                return Spectrum.Zero;
            }

            var reflect = wiW % _ng * (woW % _ng) > 0f;
            var f = Spectrum.Zero;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                var b = _bxdf[i];
                if (b.AnyFlag(flags) &&
                    (reflect && b.AnyFlag(BxDFType.Reflection) ||
                     !reflect && b.AnyFlag(BxDFType.Transmission)))
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
                if (current.AnyFlag(flags))
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
                if (current.AnyFlag(flags))
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

        public Spectrum Sample_F(
                             in Vector woWorld,
                             out Vector wiWorld,
                             Point2D u,
                             out float pdf,
                             in BxDFType type,
                             out BxDFType sampledType)
        {
            var matching = NumberOfComponents(type);
            if (matching == 0)
            {
                pdf = 0f;
                wiWorld = Vectors.Zero;
                sampledType = BxDFType.None;
                return Spectrum.Zero;
            }

            var comp = System.Math.Min((int) Floor(u[0] * matching), matching - 1);

            IBxDF bxdf = null;
            var count = comp;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                if (_bxdf[i].AnyFlag(type) && count-- == 0)
                {
                    bxdf = _bxdf[i];
                    break;
                }
            }

            var uRemapped = new Point2D(Min(u[0] * matching - comp, OneMinusEpsilon), u[1]);
            var wo = WorldToLocal(woWorld);
            if (wo.Z == 0f || bxdf is null)
            {
                pdf = 0;
                wiWorld = Vectors.Zero;
                sampledType = BxDFType.None;
                return  Spectrum.Zero;
            }

            sampledType = bxdf.Type;
            var wi = new Vector();
            var fx = bxdf.SampleF(wo, ref wi, uRemapped, out pdf, sampledType);
            if (pdf == 0f)
            {
                pdf = 0;
                wiWorld = Vectors.Zero;
                sampledType = BxDFType.None;
                return  Spectrum.Zero;
            }

            wiWorld = LocalToWorld(wi);

            if (!bxdf.AnyFlag(BxDFType.Specular) && matching > 1)
            {
                for (var i = 0; i < _nBxDFs; ++i)
                {
                    if (!ReferenceEquals(_bxdf[i], bxdf) && _bxdf[i].AnyFlag(type))
                    {
                        pdf += _bxdf[i].Pdf(wo, wi);
                    }
                }
            }

            if (matching > 1)
            {
                pdf /= matching;
            }

            if (!bxdf.AnyFlag(BxDFType.Specular))
            {
                var reflect = wiWorld % _ng * woWorld % _ng > 0f;
                var f = Spectrum.Zero;
                //f.Zero();
                for (var i = 0; i < _nBxDFs; ++i)
                {
                    if (_bxdf[i].AnyFlag(type) &&
                        (reflect && _bxdf[i].AnyFlag(BxDFType.Reflection) ||
                         !reflect && _bxdf[i].AnyFlag(BxDFType.Transmission)))
                    {
                        //f.Contribute(_bxdf[i].F(wo, wi));
                        f += _bxdf[i].F(wo, wi);
                    }
                }

                return f;
            }

            return fx;
        }

        public float Pdf(in Vector woWorld, in Vector wiWorld, BxDFType flags = BxDFType.All)
        {
            if (_nBxDFs == 0f)
            {
                return 0f;
            }

            var wo = WorldToLocal(woWorld);
            if (wo.Z == 0f)
            {
                return 0f;
            }

            var wi = WorldToLocal(wiWorld);
            var pdf = 0f;
            var matching = 0;
            for (var i = 0; i < _nBxDFs; ++i)
            {
                if (_bxdf[i].AnyFlag(flags))
                {
                    ++matching;
                    pdf += _bxdf[i].Pdf(in wo, in wi);
                }
            }

            return matching > 0 ? pdf / matching : 0f;
        }
    }
}