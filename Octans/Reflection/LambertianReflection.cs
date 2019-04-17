using System;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class LambertianReflection : IBxDF
    {
        public Spectrum R { get; private set; }

        public LambertianReflection Initialize(in Spectrum r)
        {
            R = r;
            return this;
        }

        public BxDFType Type => BxDFType.Reflection | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi) => R * InvPi;

        public Spectrum SampleF(in Vector wo, ref Vector wi, in Point sample, out float pdf, BxDFType sampleType = BxDFType.None) => throw new NotImplementedException();

        public Spectrum Rho(in Vector wo, int nSamples, in Point[] u) => R;

        public Spectrum Rho(int nSamples, in Point[] u1, in Point[] u2) => R;
    }
}