namespace Octans.Reflection
{
    // ReSharper disable once InconsistentNaming
    public interface IBxDF
    {
        BxDFType Type { get; }


        Spectrum F(in Vector wo, in Vector wi);

        Spectrum SampleF(in Vector wo,
                         ref Vector wi,
                         in Point2D u,
                         out float pdf,
                         BxDFType sampleType = BxDFType.None);

        Spectrum Rho(in Vector wo, int nSamples, in Point2D[] u);

        Spectrum Rho(int nSamples, in Point2D[] u1, in Point2D[] u2);
        float Pdf(in Vector wo, in Vector wi);
    }
}