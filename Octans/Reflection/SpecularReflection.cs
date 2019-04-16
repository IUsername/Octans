using static Octans.MathF;

namespace Octans.Reflection
{
    public class SpecularReflection : IBxDF
    {
        private readonly IFresnel _fresnel;

        public SpecularReflection(in Spectrum r, IFresnel fresnel)
        {
            R = r;
            _fresnel = fresnel;
        }

        public Spectrum R { get; }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None)
        {
            wi = new Vector(-wo.X, -wo.Y, wo.Z);
            pdf = 1f;
            return _fresnel.Evaluate(CosTheta(in wi)) * R / AbsCosTheta(in wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => Utilities.Rho(this, in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => Utilities.Rho(this, nSamples, in u1, in u2);
    }
}