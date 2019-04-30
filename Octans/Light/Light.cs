namespace Octans.Light
{
    public abstract class Light : ILight
    {
        protected Light(LightType type, Transform lightToWorld, IMedium mediumInterface, int nSamples = 1)
        {
            Type = type;
            LightToWorld = lightToWorld;
            MediumInterface = mediumInterface;
            WorldToLight = Transform.Invert(lightToWorld);
            NSamples = System.Math.Max(1, nSamples);
        }

        protected int NSamples { get; }

        protected Transform WorldToLight { get; }

        protected Transform LightToWorld { get; }

        protected IMedium MediumInterface { get; }

        public virtual void Preprocess(IScene scene)
        {
        }

        public LightType Type { get; }

        public Spectrum Le(in RayDifferential ray) => Spectrum.Zero;

        public abstract Spectrum Sample_Li(Interaction reference,
                                           Point2D u,
                                           out Vector wi,
                                           out float pdf,
                                           out VisibilityTester visibility);

        public abstract Spectrum Sample_Le(in Point2D u1,
                                           in Point2D u2,
                                           out Ray ray,
                                           out Normal nLight,
                                           out float pdfPos,
                                           out float pdfDir);

        public abstract Spectrum Power();
        public abstract void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir);
        public abstract float Pdf_Li(Interaction i, in Vector wi);
    }
}