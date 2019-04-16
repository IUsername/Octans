namespace Octans.Reflection
{
    public class SpecularReflection : IBxDF
    {
        public Spectrum R { get; }
        private readonly IFresnel _fresnel;

        public SpecularReflection(in Spectrum r, IFresnel fresnel)
        {
            R = r;
            _fresnel = fresnel;
        }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Specular;

        public Spectrum F(in Vector wo, in Vector wi) => Spectrum.Zero;

        public Spectrum SampleF(in Vector wo, ref Vector wi, in Point sample, out float pdf, BxDFType sampleType = BxDFType.None)
        {
            wi = new Vector(-wo.X, -wo.Y, wo.Z);
            pdf = 1f;
            return _fresnel.Evaluate(Utilities.CosTheta(in wi)) * R / Utilities.AbsCosTheta(in wi);
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] samples) => throw new System.NotImplementedException();

        public Spectrum Rho(int nSamples, in Point[] samples1, in Point[] samples2) => throw new System.NotImplementedException();
    }
}