using System;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class OrenNayar : IBxDF
    {
        public OrenNayar(in Spectrum r, float sigma)
        {
            R = r;
            sigma = Rad(sigma);
            var sigma2 = sigma * sigma;
            A = 1f - sigma2 / (2f * (sigma2 + 0.33f));
            B = 0.45f * sigma2 / (sigma2 + 0.09f);
        }

        public Spectrum R { get; }

        public float B { get; }

        public float A { get; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi)
        {
            var sinThetaI = SinTheta(in wi);
            var sinThetaO = SinTheta(in wo);
            var maxCos = 0f;
            if (sinThetaI > 1e-4f && sinThetaO > 1e-4f)
            {
                var sinPhiI = SinPhi(in wi);
                var cosPhiI = CosPhi(in wi);
                var sinPhiO = SinPhi(in wo);
                var cosPhiO = CosPhi(in wo);
                var dCos = cosPhiI * cosPhiO + sinPhiI * sinPhiO;
                maxCos = System.MathF.Max(0f, dCos);
            }

            var (sinAlpha, tanBeta) = AbsCosTheta(in wi) > AbsCosTheta(in wo)
                ? (sinThetaO, sinThetaI / AbsCosTheta(in wi))
                : (sinThetaI, sinThetaO / AbsCosTheta(in wo));

            return R * InvPi * (A + B * maxCos * sinAlpha * tanBeta);
        }

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);
    }
}