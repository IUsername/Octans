using static Octans.MathF;

namespace Octans.Reflection
{
    public class LambertianTransmission : IBxDF
    {
        public Spectrum T { get; private set; }

        public BxDFType Type => BxDFType.Transmission | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi) => T * InvPi;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            wi = Sampling.Utilities.CosineSampleHemisphere(u);
            if (wo.Z > 0f)
            {
                wi = new Vector(wi.X, wi.Y, -wi.Z);
            }

            pdf = Pdf(wo, wi);
            return F(wo, wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => T;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => T;

        public float Pdf(in Vector wo, in Vector wi) => !IsInSameHemisphere(wo, wi) ? AbsCosTheta(wi) * InvPi : 0f;

        public LambertianTransmission Initialize(in Spectrum t)
        {
            T = t;
            return this;
        }
    }
}