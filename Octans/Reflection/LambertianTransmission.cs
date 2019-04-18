using System;
using static Octans.MathF;

namespace Octans.Reflection
{
    public class LambertianTransmission : IBxDF
    {
        public Spectrum T { get; }

        public LambertianTransmission(in Spectrum t)
        {
            T = t;
        }

        public BxDFType Type => BxDFType.Transmission | BxDFType.Diffuse;

        public Spectrum F(in Vector wo, in Vector wi) => T * InvPi;

        public Spectrum SampleF(in Vector wo, ref Vector wi, in Point2D sample, out float pdf, BxDFType sampleType = BxDFType.None)
        {
            // TODO
            throw new NotImplementedException();
        }

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => T;

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) => T;
    }
}