namespace Octans.Light
{
    public sealed class PointLight : ILight
    {
        public Transform Light2World { get; }
        private readonly Point _pLight;
        public Spectrum I { get; }

        public PointLight(Transform light2World, Spectrum i)
        {
            Light2World = light2World;
            I = i;
            _pLight = Light2World * Point.Zero;
        }
        public void Preprocess(IScene scene)
        {
           
        }

        public LightType Type => LightType.DeltaPosition;

        public Spectrum Le(in RayDifferential ray) => Spectrum.Zero;

        public Spectrum Sample_Li(Interaction it, Point2D u, out Vector wi, out float pdf, out VisibilityTester visibility)
        {
            wi = (_pLight - it.P).Normalize();
            pdf = 1f;
            visibility = new VisibilityTester(it, new Interaction(_pLight));
            var d2 = Point.DistanceSqr(_pLight, it.P);
            return I / d2;
        }

        public Spectrum Sample_Le(in Point2D u1, in Point2D u2, out Ray ray, out Normal nLight, out float pdfPos, out float pdfDir)
        {
            ray = new Ray(_pLight, Sampling.Utilities.UniformSampleSphere(u1));
            nLight = (Normal) ray.Direction;
            pdfPos = 1f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
            return I;
        }

        public Spectrum Power() => I * (4 * System.MathF.PI);

        public void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            pdfPos = 0f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
        }

        public float Pdf_Li(Interaction i, in Vector wi) => 0f;
    }
}