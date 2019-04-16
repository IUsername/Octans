namespace Octans.Reflection
{
    // ReSharper disable once InconsistentNaming
    public interface IBxDF
    {
        BxDFType Type { get; }


        Spectrum F(in Vector wo, in Vector wi);

        Spectrum SampleF(in Vector wo,
                         ref Vector wi,
                         in Point sample,
                         out float pdf,
                         BxDFType sampleType = BxDFType.None);

        Spectrum Rho(in Vector wo, int nSamples, in Point[] samples);

        Spectrum Rho(int nSamples, in Point[] samples1, in Point[] samples2);
    }
}