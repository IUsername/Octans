using static Octans.MathF;
using static Octans.Reflection.Utilities;

namespace Octans.Reflection
{
    public class SpecularTransmission : IBxDF
    {
        private readonly float _etaA;
        private readonly float _etaB;
        private readonly FresnelDielectric _fresnel = new FresnelDielectric();

        public SpecularTransmission(in Spectrum t, float etaA, float etaB, TransportMode mode)
        {
            _etaA = etaA;
            _etaB = etaB;
            Mode = mode;
            T = t;
            _fresnel.Initialize(etaA, etaB);
        }

        public TransportMode Mode { get; }

        public Spectrum T { get; }

        public BxDFType Type => BxDFType.Transmission | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            var isEntering = CosTheta(in wo) > 0f;
            var (etaI, etaT) = isEntering ? (_etaA, _etaB) : (_etaB, _etaA);
            pdf = 1f;
            if (!Refract(in wo, Normal.FaceForward(Normals.ZPos, in wo), etaI / etaT, ref wi))
            {
                return Spectrum.Zero;
            }

            var ft = T * (Spectrum.One - _fresnel.Evaluate(CosTheta(in wi)));
            return ft / AbsCosTheta(in wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) =>
            IsInSameHemisphere(wo, wi) ? AbsCosTheta(wi) * InvPi : 0;
    }
}