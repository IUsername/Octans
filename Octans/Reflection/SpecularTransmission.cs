using static Octans.MathF;

namespace Octans.Reflection
{
    public class SpecularTransmission : IBxDF
    {
        public float EtaA { get; private set; }
        public float EtaB { get; private set; }
        public FresnelDielectric Fresnel { get; } = new FresnelDielectric();

        public TransportMode Mode { get; private set; }

        public Spectrum T { get; private set; }

        public BxDFType Type => BxDFType.Transmission | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            var isEntering = CosTheta(in wo) > 0f;
            var (etaI, etaT) = isEntering ? (EtaA, EtaB) : (EtaB, EtaA);
            pdf = 1f;
            if (!Refract(in wo, Normal.FaceForward(Normals.ZPos, in wo), etaI / etaT, ref wi))
            {
                return Spectrum.Zero;
            }

            var ft = T * (Spectrum.One - Fresnel.Evaluate(CosTheta(in wi)));
            if (Mode == TransportMode.Radiance)
            {
                ft *= (etaI * etaI) / (etaT * etaT);
            }
            return ft / AbsCosTheta(in wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) => 0f;

        public SpecularTransmission Initialize(in Spectrum t, float etaA, float etaB, TransportMode mode)
        {
            EtaA = etaA;
            EtaB = etaB;
            Mode = mode;
            T = t;
            Fresnel.Initialize(etaA, etaB);
            return this;
        }
    }
}