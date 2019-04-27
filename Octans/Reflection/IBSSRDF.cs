using System;
using static System.MathF;
using static Octans.Math;
using static Octans.MathF;

namespace Octans.Reflection
{
    public interface IBSSRDF
    {
        Spectrum S(SurfaceInteraction pi, in Vector wi);

        Spectrum SampleS(IScene scene, float u1, Point2D u2, IObjectArena arena, ref SurfaceInteraction si, out float pdf);
    }

    public static class BSSRDFTools
    {
        public static float FresnelMoment1(float eta)
        {
            float eta2 = eta * eta,
                  eta3 = eta2 * eta,
                  eta4 = eta3 * eta,
                  eta5 = eta4 * eta;

            if (eta < 1f)
            {
                return 0.45966f - 1.73965f * eta + 3.37668f * eta2 - 3.904945f * eta3 +
                       2.49277f * eta4 - 0.68441f * eta5;
            }

            return -4.61686f + 11.1136f * eta - 10.4646f * eta2 + 5.11455f * eta3 -
                   1.27198f * eta4 + 0.12746f * eta5;
        }
    }

    //public class BSSRDF : IBSSRDF
    //{
    //    public BSSRDF Initialize(SurfaceInteraction si, float eta)
    //    {
    //        SI = si;
    //        Eta = eta;
    //    }

    //    public float Eta { get; private set; }

    //    public SurfaceInteraction SI { get; private set; }
    //}

    public abstract class SeparableBSSRDF : IBSSRDF
    {
        public float Eta { get; private set; }

        protected internal TransportMode Mode { get; private set; }

        protected IMaterial Material { get; private set; }

        protected Vector TS { get; private set; }

        protected Vector SS { get; private set; }

        protected Normal NS { get; private set; }

        public SurfaceInteraction PO { get; private set; }

        public virtual Spectrum S(SurfaceInteraction pi, in Vector wi)
        {
            var ft = FresnelDielectric.FrDielectric(CosTheta(PO.Wo), 1f, Eta);
            return (1f - ft) * Sp(pi) * Sw(wi);
        }

        public Spectrum SampleS(IScene scene,
                                float u1,
                                Point2D u2,
                                IObjectArena arena,
                                ref SurfaceInteraction si,
                                out float pdf)
        {
            var Sp = SampleSp(scene, u1, u2, arena, ref si, out pdf);
            if (Sp.IsBlack())
            {
                return Sp;
            }

            si.BSDF.Initialize(si);
            si.BSDF.Add(arena.Create<SeparatableBSSRDFAdapter>().Initialize(this));
            si.Wo = (Vector) si.ShadingGeometry.N;
            return Sp;
        }

        private Spectrum SampleSp(IScene scene,
                                  float u1,
                                  in Point2D u2,
                                  IObjectArena arena,
                                  ref SurfaceInteraction pi,
                                  out float pdf)
        {
            Vector vx, vy, vz;
            if (u1 < 0.5f)
            {
                vx = SS;
                vy = TS;
                vz = (Vector) NS;
                u1 *= 2f;
            }
            else if (u1 < 0.75f)
            {
                vx = TS;
                vy = (Vector) NS;
                vz = SS;
                u1 = (u1 - 0.5f) * 4f;
            }
            else
            {
                vx = (Vector) NS;
                vy = SS;
                vz = TS;
                u1 = (u1 - 0.75f) * 4f;
            }

            // Choose spectral channel
            var ch = Clamp(0, Spectrum.Samples - 1, (int) (u1 * Spectrum.Samples));
            u1 = u1 * Spectrum.Samples - ch;

            // Sample BSSRDF profile in polar coordinates
            var r = SampleSr(ch, u2[0]);
            if (r < 0f)
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            var phi = 2f * PI * u2[1];

            // Compute BSSRDF profile bounds and intersection height
            var rMax = SampleSr(ch, 0.999f);
            if (r >= rMax)
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            var l = 2f * Sqrt(rMax * rMax - r * r);


            // Compute BSSRDF sampling ray segment
            var b = new Interaction
            {
                P = PO.P + r * (vx * Cos(phi) + vy * Sin(phi)) - l * vz * 0.5f
            };
            // TODO: time
            var pTarget = b.P + l * vz;

            var chain = arena.Create<IntersectionChain>();
            chain.Si = new SurfaceInteraction();
            var ptr = chain;
            var nFound = 0;
            while (true)
            {
                var ray = b.SpawnRayTo(pTarget);
                if (ray.Direction == Vectors.Zero || !scene.Intersect(ray, ref ptr.Si))
                {
                    break;
                }

                b = ptr.Si;
                if (!ReferenceEquals(ptr.Si.Primitive.Material, Material))
                {
                    continue;
                }

                var next = arena.Create<IntersectionChain>();
                next.Si = new SurfaceInteraction();
                ptr.Next = next;
                ptr = next;
                nFound++;
            }

            if (nFound == 0)
            {
                pdf = 0f;
                return Spectrum.Zero;
            }

            // Randomly choose one of several intersections during BSSRDF sampling
            var selected = Clamp(0, nFound - 1, (int) (u1 * nFound));
            while (selected-- > 0)
            {
                chain = chain.Next;
            }

            pi = chain.Si;
            pdf = PdfSp(pi) / nFound;
            return Sp(pi);
        }

        public abstract float SampleSr(in int ch, float u);

        private float PdfSp(SurfaceInteraction pi)
        {
            var d = PO.P - pi.P;
            var dLocal = new Vector(SS % d, TS % d, NS % d);
            var nLocal = new Vector(SS % pi.N, TS % pi.N, NS % pi.N);

            var rProj = new[]
            {
                Sqrt(dLocal.Y * dLocal.Y + dLocal.Z * dLocal.Z),
                Sqrt(dLocal.Z * dLocal.Z + dLocal.X * dLocal.X),
                Sqrt(dLocal.X * dLocal.X + dLocal.Y * dLocal.Y)
            };

            var pdf = 0f;
            var axisProb = new[] {0.25f, 0.25f, 0.5f};
            var chProb = 1f / Spectrum.Samples;
            for (var axis = 0; axis < 3; ++axis)
            {
                for (var ch = 0; ch < Spectrum.Samples; ++ch)
                {
                    pdf += PdfSr(ch, rProj[axis]) * Abs(nLocal[axis]) * chProb * axisProb[axis];
                }
            }

            return pdf;
        }

        public abstract float PdfSr(in int ch, float r);

        protected SeparableBSSRDF Initialize(SurfaceInteraction po, float eta, IMaterial material, TransportMode mode)
        {
            PO = po;
            Eta = eta;
            NS = po.ShadingGeometry.N;
            SS = po.ShadingGeometry.Dpdu.Normalize();
            TS = Vector.Cross(NS, SS);
            Material = material;
            Mode = mode;
            return this;
        }

        internal Spectrum Sw(in Vector w)
        {
            var c = 1f - 2f * BSSRDFTools.FresnelMoment1(1f / Eta);
            return new Spectrum((1f - FresnelDielectric.FrDielectric(CosTheta(w), 1f, Eta)) / (c * PI));
        }

        protected Spectrum Sp(SurfaceInteraction pi) => Sr(Point.Distance(PO.P, pi.P));

        public abstract Spectrum Sr(float r);

        protected class IntersectionChain
        {
            // TODO: Nullable reference type
            public IntersectionChain Next;
            public SurfaceInteraction Si;
        }
    }

    public class SeparatableBSSRDFAdapter : IBxDF
    {
        public SeparableBSSRDF BSSRDF { get; private set; }
        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var f = BSSRDF.Sw(wi);
            if (BSSRDF.Mode == TransportMode.Radiance)
            {
                f *= BSSRDF.Eta * BSSRDF.Eta;
            }

            return f;
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            this.CosineSampleHemisphereF(in wo, ref wi, in u, out pdf);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(wo, wi);

        public IBxDF Initialize(SeparableBSSRDF bssrdf)
        {
            BSSRDF = bssrdf;
            return this;
        }
    }
}