namespace Octans.Reflection
{
    // ReSharper disable once InconsistentNaming
    public class ScaledBxDF : IBxDF
    {
        private readonly IBxDF _bxdf;
        private readonly Spectrum _scale;

        public ScaledBxDF(IBxDF bxdf, in Spectrum scale) 
        {
            _bxdf = bxdf;
            _scale = scale;
            Type = _bxdf.Type;
        }

        public BxDFType Type { get; }

        public Spectrum F(in Vector wo, in Vector wi)
        {
            return _scale * _bxdf.F(in wo, in wi);
        }

        public  Spectrum SampleF(in Vector wo, ref Vector wi, in Point sample, out float pdf, BxDFType sampleType = BxDFType.None)
        {
            return _scale * _bxdf.SampleF(in wo, ref wi, in sample, out pdf, sampleType);
        }

        public  Spectrum Rho(in Vector wo, int nSamples, in Point[] samples)
        {
            return _scale * _bxdf.Rho(in wo, nSamples, in samples);
        }

        public Spectrum Rho(int nSamples, in Point[] samples1, in Point[] samples2)
        {
            return _scale * _bxdf.Rho(nSamples, in samples1, in samples2);
        }
    }
}