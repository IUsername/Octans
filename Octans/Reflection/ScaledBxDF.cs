namespace Octans.Reflection
{
    // ReSharper disable once InconsistentNaming
    public class ScaledBxDF : IBxDF
    {
        private IBxDF _bxdf;
        private Spectrum _scale;

        public BxDFType Type { get; private set; }

        public Spectrum F(in Vector wo, in Vector wi) => _scale * _bxdf.F(in wo, in wi);

        public Spectrum SampleF(in Vector wo,
                                ref Vector wi,
                                in Point2D sample,
                                out float pdf,
                                BxDFType sampleType = BxDFType.None) =>
            _scale * _bxdf.SampleF(in wo, ref wi, in sample, out pdf, sampleType);

        public Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u) => _scale * _bxdf.Rho(in wo, nSamples, in u);

        public Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2) =>
            _scale * _bxdf.Rho(nSamples, in u1, in u2);

        public float Pdf(in Vector wo, in Vector wi) => _bxdf.Pdf(in wo, in wi);

        public ScaledBxDF Initialize(IBxDF bxdf, in Spectrum scale)
        {
            _bxdf = bxdf;
            _scale = scale;
            Type = _bxdf.Type;
            return this;
        }
    }
}