namespace Octans.Reflection
{
    // ReSharper disable once InconsistentNaming
    public class ScaledBxDF : IBxDF
    {
        private IBxDF _bxdf;
        private Spectrum _scale;
        
        public ScaledBxDF Initialize(IBxDF bxdf, in Spectrum scale)
        {
            _bxdf = bxdf;
            _scale = scale;
            Type = _bxdf.Type;
            return this;
        }

        public BxDFType Type { get; private set; }

        public Spectrum F(in Vector wo, in Vector wi)
        {
            return _scale * _bxdf.F(in wo, in wi);
        }

        public  Spectrum SampleF(in Vector wo, ref Vector wi, in Point2D sample, out float pdf, BxDFType sampleType = BxDFType.None)
        {
            return _scale * _bxdf.SampleF(in wo, ref wi, in sample, out pdf, sampleType);
        }

        public  Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u)
        {
            return _scale * _bxdf.Rho(in wo, nSamples, in u);
        }

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2)
        {
            return _scale * _bxdf.Rho(nSamples, in u1, in u2);
        }
    }
}