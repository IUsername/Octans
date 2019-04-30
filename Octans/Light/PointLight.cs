namespace Octans.Light
{
    public sealed class PointLight : Light
    {
        private readonly Point _pLight;

        public PointLight(Transform lightToWorld, IMedium m, Spectrum i) 
            : base(LightType.DeltaPosition, lightToWorld, m)
        {
            I = i;
            _pLight = lightToWorld * Point.Zero;
        }

        public Spectrum I { get; }


        public override Spectrum Sample_Li(Interaction reference,
                                           Point2D u,
                                           out Vector wi,
                                           out float pdf,
                                           out VisibilityTester visibility)
        {
            wi = (_pLight - reference.P).Normalize();
            pdf = 1f;
            visibility = new VisibilityTester(reference, new Interaction(_pLight));
            var d2 = Point.DistanceSqr(_pLight, reference.P);
            return I / d2;
        }

        public override Spectrum Sample_Le(in Point2D u1,
                                           in Point2D u2,
                                           out Ray ray,
                                           out Normal nLight,
                                           out float pdfPos,
                                           out float pdfDir)
        {
            ray = new Ray(_pLight, Sampling.Utilities.UniformSampleSphere(u1));
            nLight = (Normal) ray.Direction;
            pdfPos = 1f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
            return I;
        }

        public override Spectrum Power() => I * (4 * System.MathF.PI);

        public override void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            pdfPos = 0f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
        }

        public override float Pdf_Li(Interaction i, in Vector wi) => 0f;
    }
}