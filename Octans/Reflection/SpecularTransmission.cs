using System;
using static Octans.Reflection.Utilities;

namespace Octans.Reflection
{
    public class SpecularTransmission : IBxDF
    {
        private readonly float _etaA;
        private readonly float _etaB;
        private readonly FresnelDielectric _fresnel;

        public SpecularTransmission(in Spectrum t, float etaA, float etaB, TransportMode mode)
        {
            _etaA = etaA;
            _etaB = etaB;
            Mode = mode;
            T = t;
            _fresnel = new FresnelDielectric(etaA, etaB);
        }

        public TransportMode Mode { get; }

        public Spectrum T { get; }

        public BxDFType Type => BxDFType.Transmission | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
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

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] samples) => throw new NotImplementedException();

        public Spectrum Rho(int nSamples, in Point[] samples1, in Point[] samples2) =>
            throw new NotImplementedException();
    }
}