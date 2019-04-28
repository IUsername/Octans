using Octans.Reflection;
using Octans.Reflection.Microfacet;
using static System.MathF;
using static Octans.Material.Utilities;
using static Octans.MathF;

namespace Octans.Material
{
    public sealed class DisneyMaterial : IMaterial
    {
        public DisneyMaterial(
            ITexture2<Spectrum> color,
            ITexture2<float> metallic,
            ITexture2<float> eta,
            ITexture2<float> roughness,
            ITexture2<float> specularTint,
            ITexture2<float> anisotropic,
            ITexture2<float> sheen,
            ITexture2<float> sheenTint,
            ITexture2<float> clearcoat,
            ITexture2<float> clearcoatGloss,
            ITexture2<float> specTrans,
            ITexture2<Spectrum> scatterDistance,
            bool isThin,
            ITexture2<float> flatness,
            ITexture2<float> diffTrans,
            ITexture2<float> bumpMap
        )
        {
            Color = color;
            Metallic = metallic;
            Eta = eta;
            Roughness = roughness;
            SpecularTint = specularTint;
            Anisotropic = anisotropic;
            Sheen = sheen;
            SheenTint = sheenTint;
            Clearcoat = clearcoat;
            ClearcoatGloss = clearcoatGloss;
            SpecTrans = specTrans;
            ScatterDistance = scatterDistance;
            IsThin = isThin;
            Flatness = flatness;
            DiffTrans = diffTrans;
            BumpMap = bumpMap;
        }

        public ITexture2<float> SpecTrans { get; }
        private ITexture2<Spectrum> Color { get; }
        private ITexture2<float> Metallic { get; }
        private ITexture2<float> Eta { get; }
        private ITexture2<float> Roughness { get; }
        private ITexture2<float> SpecularTint { get; }
        private ITexture2<float> Anisotropic { get; }
        private ITexture2<float> Sheen { get; }
        private ITexture2<float> SheenTint { get; }
        private ITexture2<float> Clearcoat { get; }
        private ITexture2<float> ClearcoatGloss { get; }
        private ITexture2<Spectrum> ScatterDistance { get; }
        private bool IsThin { get; }
        private ITexture2<float> Flatness { get; }
        private ITexture2<float> DiffTrans { get; }
        private ITexture2<float> BumpMap { get; }

        public void ComputeScatteringFunctions(SurfaceInteraction si,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               bool allowMultipleLobes)
        {
            BumpMap?.Bump(si);

            si.BSDF.Initialize(si);

            // Diffuse
            var c = Color.Evaluate(si).Clamp();
            var metallicWeight = Metallic.Evaluate(si);
            var e = Eta.Evaluate(si);
            var strans = SpecTrans.Evaluate(si);
            var diffuseWeight = (1f - metallicWeight) * (1f - strans);
            var dt = DiffTrans.Evaluate(si) / 2f;
            var rough = Roughness.Evaluate(si);
            var lum = c.YComponent();
            var Ctint = lum > 0f ? c / lum : Spectrum.One;

            if (diffuseWeight > 0f)
            {
                if (IsThin)
                {
                    var flat = Flatness.Evaluate(si);
                    si.BSDF.Add(arena.Create<DisneyDiffuse>().Initialize(diffuseWeight * (1f - flat) * (1 - dt) * c));
                    si.BSDF.Add(arena.Create<DisneyFakeSS>().Initialize(diffuseWeight * flat * (1f - dt) * c, rough));
                }
                else
                {
                    var sd = ScatterDistance.Evaluate(si);
                    if (sd.IsBlack())
                    {
                        si.BSDF.Add(arena.Create<DisneyDiffuse>().Initialize(diffuseWeight * c));
                    }
                    else
                    {
                        // The line below was the original code but produces some odd results.
                        si.BSDF.Add(arena.Create<SpecularTransmission>().Initialize(Spectrum.One, 1f, e, mode));
                        si.BSSRDF = arena.Create<DisneyBSSRDF>().Initialize(diffuseWeight * c, sd, si, e, this, mode);
                    }
                }

                // Retro-reflection.
                si.BSDF.Add(arena.Create<DisneyRetro>().Initialize(diffuseWeight * c, rough));

                // Sheen
                var sheenWeight = Sheen.Evaluate(si);
                if (sheenWeight > 0f)
                {
                    var stint = SheenTint.Evaluate(si);
                    var Csheen = Spectrum.Lerp(Spectrum.One, Ctint, stint);
                    si.BSDF.Add(arena.Create<DisneySheen>().Initialize(diffuseWeight * sheenWeight * Csheen));
                }
            }

            // Microfacet distribution
            var aspect = Sqrt(1f - Anisotropic.Evaluate(si) * 0.9f);
            var ax = Max(0.001f, Sqr(rough) / aspect);
            var ay = Max(0.001f, Sqr(rough) * aspect);
            var dist = arena.Create<DisneyMicrofacetDistribution>().Initialize(ax, ay);

            // Specular = Trowbridge-Reitz with modified Fresnel function.
            var specTint = SpecularTint.Evaluate(si);
            var Cspec0 = Spectrum.Lerp(SchlickR0FromEta(e) * Spectrum.Lerp(Spectrum.One, Ctint, specTint), c,
                                       metallicWeight);
            var fresnel = arena.Create<DisneyFresnel>().Initialize(Cspec0, metallicWeight, e);
            si.BSDF.Add(arena.Create<MicrofacetReflection>().Initialize(c, dist, fresnel));

            // Clearcoat
            var cc = Clearcoat.Evaluate(si);
            if (cc > 0f)
            {
                si.BSDF.Add(arena.Create<DisneyClearcoat>()
                                 .Initialize(cc, Lerp(0.1f, 0.001f, ClearcoatGloss.Evaluate(si))));
            }

            // BTDF
            if (strans > 0f)
            {
                // Walter et al's model, with the provided transmissive term scaled
                // by sqrt(color), so that after two refractions, we're back to the
                // provided color.
                var T = strans * c.Sqrt();
                if (IsThin)
                {
                    var rScaled = (0.65f * e - 0.35f) * rough;
                    var atx = Max(0.001f, Sqr(rScaled) / aspect);
                    var aty = Max(0.001f, Sqr(rScaled) * aspect);
                    var scaledDist = arena.Create<TrowbridgeReitzDistribution>().Initialize(atx, aty);
                    si.BSDF.Add(arena.Create<MicrofacetTransmission>().Initialize(T, scaledDist, 1f, e, mode));
                }
                else
                {
                    si.BSDF.Add(arena.Create<MicrofacetTransmission>().Initialize(T, dist, 1f, e, mode));
                }
            }

            if (IsThin)
            {
                si.BSDF.Add(arena.Create<LambertianTransmission>().Initialize(dt * c));
            }
        }
    }

    public class DisneyDiffuse : IBxDF
    {
        public Spectrum R { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var fo = SchlickWeight(AbsCosTheta(wo));
            var fi = SchlickWeight(AbsCosTheta(wi));

            // Diffuse fresnel - go from 1 at normal incidence to .5 at grazing.
            // Burley 2015, eq (4).
            return InvPi * (1f - fo / 2f) * (1f - fi / 2f) * R;
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            this.CosineSampleHemisphereF(in wo, ref wi, in u, out pdf);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => R;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => R;

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(in wo, in wi);

        public DisneyDiffuse Initialize(Spectrum r)
        {
            R = r;
            return this;
        }
    }

    public class DisneyFakeSS : IBxDF
    {
        public float Roughness { get; private set; }

        public Spectrum R { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var wh = wi + wo;
            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }

            wh = wh.Normalize();
            var cosThetaD = wi % wh;

            var fss90 = cosThetaD * cosThetaD * Roughness;
            var fo = SchlickWeight(AbsCosTheta(wo));
            var fi = SchlickWeight(AbsCosTheta(wi));
            var fss = Lerp(1f, fss90, fo) * Lerp(1f, fss90, fi);

            // Scale to approximate albedo value.
            var ss = 1.25f * (fss * (1f / (AbsCosTheta(wo) + AbsCosTheta(wi)) - 0.5f) + 0.5f);
            return R * (InvPi * ss);
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            this.CosineSampleHemisphereF(in wo, ref wi, in u, out pdf);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => R;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => R;

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(in wo, in wi);

        public DisneyFakeSS Initialize(Spectrum r, float roughness)
        {
            R = r;
            Roughness = roughness;
            return this;
        }
    }

    public class DisneyRetro : IBxDF
    {
        public float Roughness { get; private set; }

        public Spectrum R { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var wh = wi + wo;
            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }

            wh = wh.Normalize();
            var cosThetaD = wi % wh;
            var fo = SchlickWeight(AbsCosTheta(wo));
            var fi = SchlickWeight(AbsCosTheta(wi));
            var Rr = 2f * Roughness * cosThetaD * cosThetaD;

            // Burley 2015, eq (4).
            return R * (InvPi * Rr * (fo + fi + fo * fi * (Rr - 1)));
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            this.CosineSampleHemisphereF(in wo, ref wi, in u, out pdf);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => R;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => R;

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(in wo, in wi);

        public DisneyRetro Initialize(Spectrum r, float roughness)
        {
            R = r;
            Roughness = roughness;
            return this;
        }
    }

    public class DisneySheen : IBxDF
    {
        public Spectrum R { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var wh = wi + wo;
            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }

            wh = wh.Normalize();
            var cosThetaD = wi % wh;
            return R * SchlickWeight(cosThetaD);
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            this.CosineSampleHemisphereF(in wo, ref wi, in u, out pdf);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => R;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => R;

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(in wo, in wi);

        public DisneySheen Initialize(Spectrum r)
        {
            R = r;
            return this;
        }
    }

    public class DisneyClearcoat : IBxDF
    {
        public float Gloss { get; private set; }

        public float Weight { get; private set; }
        public BxDFType Type => BxDFType.Reflection | BxDFType.Glossy;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var wh = wi + wo;
            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return Spectrum.Zero;
            }

            wh = wh.Normalize();

            // Clearcoat has ior = 1.5 hardcoded -> F0 = 0.04. It then uses the
            // GTR1 distribution, which has even fatter tails than Trowbridge-Reitz
            // (which is GTR2).
            var Dr = GTR1(AbsCosTheta(wh), Gloss);
            var Fr = FrSchlick(0.04f, wo % wh);

            // The geometric term always based on alpha = 0.25.
            var Gr = SmithG_GGX(AbsCosTheta(wo), 0.25f) * SmithG_GGX(AbsCosTheta(wi), 0.25f);
            return new Spectrum(Weight * Gr * Fr * Dr / 4f);
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            pdf = 0f;
            if (wo.Z == 0f)
            {
                return Spectrum.Zero;
            }

            var alpha2 = Gloss * Gloss;
            var cosTheta = Sqrt(Max(0f, (1f - Pow(alpha2, 1f - u[0])) / (1f - alpha2)));
            var sinTheta = Sqrt(Max(0f, 1f - cosTheta * cosTheta));
            var phi = 2f * PI * u[1];
            var wh = SphericalDirection(sinTheta, cosTheta, phi);
            if (!IsInSameHemisphere(wo, wh))
            {
                wh = -wh;
            }

            wi = Reflect(wo, wh);
            if (!IsInSameHemisphere(wo, wi))
            {
                return Spectrum.Zero;
            }

            pdf = Pdf(wo, wi);
            return F(wo, wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi)
        {
            var wh = wi + wo;
            if (wh.X == 0f && wh.Y == 0f && wh.Z == 0f)
            {
                return 0f;
            }

            wh = wh.Normalize();

            // The sampling routine samples wh exactly from the GTR1 distribution.
            // Thus, the final value of the PDF is just the value of the
            // distribution for wh converted to a measure with respect to the
            // surface normal.
            var Dr = GTR1(AbsCosTheta(wh), Gloss);
            return Dr * AbsCosTheta(wh) / (4f * wo % wh);
        }

        public DisneyClearcoat Initialize(float weight, float gloss)
        {
            Weight = weight;
            Gloss = gloss;
            return this;
        }

        private static float GTR1(float cosTheta, float alpha)
        {
            var alpha2 = alpha * alpha;
            return (alpha2 - 1f) /
                   (PI * Log(alpha2) * (1f + (alpha2 - 1f) * cosTheta * cosTheta));
        }

        private static float SmithG_GGX(float cosTheta, float alpha)
        {
            var alpha2 = alpha * alpha;
            var cosTheta2 = cosTheta * cosTheta;
            return 1f / (cosTheta + Sqrt(alpha2 + cosTheta2 - alpha2 * cosTheta2));
        }
    }

    public class DisneyFresnel : IFresnel
    {
        public float Eta { get; private set; }

        public float Metallic { get; private set; }

        public Spectrum R0 { get; private set; }

        public Spectrum Evaluate(float cosI) =>
            Spectrum.Lerp(
                new Spectrum(FresnelDielectric.FrDielectric(cosI, 1f, Eta)),
                FrSchlick(R0, cosI),
                Metallic);

        public DisneyFresnel Initialize(Spectrum r0, float metallic, float eta)
        {
            R0 = r0;
            Metallic = metallic;
            Eta = eta;
            return this;
        }
    }

    public sealed class DisneyMicrofacetDistribution : TrowbridgeReitzDistribution
    {
        public DisneyMicrofacetDistribution Initialize(float alphaX, float alphaY)
        {
            base.Initialize(alphaX, alphaY);
            return this;
        }

        public override float G(in Vector wo, in Vector wi) => G1(wo) * G1(wi);
    }

    public sealed class DisneyBSSRDF : SeparableBSSRDF
    {
        public Spectrum D { get; private set; }

        public Spectrum R { get; private set; }

        public DisneyBSSRDF Initialize(Spectrum r,
                                       Spectrum d,
                                       SurfaceInteraction po,
                                       float eta,
                                       IMaterial material,
                                       TransportMode mode)
        {
            base.Initialize(po, eta, material, mode);
            R = r;
            D = d * 0.2f;
            return this;
        }

        public override Spectrum S(SurfaceInteraction pi, in Vector wi)
        {
            var a = (pi.P - PO.P).Normalize();
            var fade = 1f;
            var n = (Vector) PO.ShadingGeometry.N;
            var cosTheta = a % n;
            if (cosTheta > 0f)
            {
                var sinTheta = Sqrt(Max(0f, 1f - cosTheta * cosTheta));
                var a2 = n * sinTheta - (a - n * cosTheta) * cosTheta / sinTheta;
                fade = Max(0f, PO.ShadingGeometry.N % a2);
            }

            var fo = SchlickWeight(AbsCosTheta(PO.Wo));
            var fi = SchlickWeight(AbsCosTheta(wi));

            return fade * (1 - fo / 2f) * (1f - fi / 2f) * Sp(pi) / PI;
        }

        public override float SampleSr(in int ch, float u)
        {
            if (u < 0.25f)
            {
                u = Min(u * 4f, OneMinusEpsilon);
                return D[ch] * Log(1f / (1f - u));
            }

            u = Min((u - 0.25f) / 0.75f, OneMinusEpsilon);
            return 3f * D[ch] * Log(1f / (1f - u));
        }

        public override float PdfSr(in int ch, float r)
        {
            if (r < 1e-6f)
            {
                r = 1e-6f;
            }

            // Weight the two individual PDFs as per the sampling frequency in
            // Sample_Sr().
            return 0.25f * Exp(-r / D[ch]) / (2f * PI * D[ch] * r) +
                   0.75f * Exp(-r / (3f * D[ch])) / (6f * PI * D[ch] * r);
        }

        public override Spectrum Sr(float r)
        {
            if (r < 1e-6f)
            {
                r = 1e-6f;
            }

            var rS = new Spectrum(-r);
            return R * ((rS / D).Exp() + (rS / (3f * D)).Exp()) / (8f * PI * D * r);
        }
    }
}