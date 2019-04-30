namespace Octans
{
    public interface ILight
    {
        LightType Type { get; }
        void Preprocess(IScene scene);
        Spectrum Le(in RayDifferential ray);

        Spectrum Sample_Li(Interaction reference,
                           Point2D u,
                           out Vector wi,
                           out float pdf,
                           out VisibilityTester visibility);

        Spectrum Sample_Le(in Point2D u1,
                           in Point2D u2,
                           out Ray ray,
                           out Normal nLight,
                           out float pdfPos,
                           out float pdfDir);

        Spectrum Power();

        void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir);
        float Pdf_Li(Interaction i, in Vector wi);
    }
}