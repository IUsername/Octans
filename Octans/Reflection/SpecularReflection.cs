using static Octans.MathF;

namespace Octans.Reflection
{
    public class SpecularReflection : IBxDF
    {
        private  IFresnel _fresnel;

        public SpecularReflection Initialize(Spectrum r, IFresnel fresnel)
        {
            R = r;
            _fresnel = fresnel;
            return this;
        }

        public Spectrum R { get; private set; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D u,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            wi = new Vector(-wo.X, -wo.Y, wo.Z);
            pdf = 1f;
            return _fresnel.Evaluate(CosTheta(in wi)) * R / AbsCosTheta(in wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => this.RhoValue(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => this.RhoValue(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) => this.LambertianPdfValue(in wo, in wi);
    }
}