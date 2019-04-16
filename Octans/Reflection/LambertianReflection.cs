using System;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class LambertianReflection : IBxDF
    {
        public Spectrum R { get; }

        public LambertianReflection(in Spectrum r)
        {
            R = r;
        }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi) => R * InvPi;

        public Spectrum SampleF(in Vector wo, ref Vector wi, in Point sample, out float pdf, BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] samples) => R;

        public Spectrum Rho(int nSamples, in Point[] samples1, in Point[] samples2) => R;
    }
}