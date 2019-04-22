namespace Octans
{
    public interface ILight
    {
        Point Position { get; }
        Color Intensity { get; }
        Point[] SamplePoints { get; }

        int Samples { get; }
      
    }

    public interface ILight2
    {
        void Preprocess(IScene scene);

        LightType Type { get; }
        Spectrum Le(in RayDifferential ray);
        Spectrum Sample_Li(SurfaceInteraction si, Point2D u, out Vector wi, out float pdf, out VisibilityTester visibility);
        Spectrum Sample_Le(in Point2D u1, in Point2D u2, out Ray ray, out Normal nLight, out float pdfPos, out float pdfDir);

        Spectrum Power();

        void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir);
        float Pdf_Li(Interaction i, in Vector wi);
    }

    public sealed class PointLight2 : ILight2
    {
        public Transform Light2World { get; }
        private readonly Point _pLight;
        public Spectrum I { get; }

        public PointLight2(Transform light2World, Spectrum i)
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

        public Spectrum Sample_Li(SurfaceInteraction si, Point2D u, out Vector wi, out float pdf, out VisibilityTester visibility)
        {
            wi = (_pLight - si.P).Normalize();
            pdf = 1f;
            visibility = new VisibilityTester(si, new Interaction(_pLight));
            return I / Point.DistanceSqr(_pLight, si.P);
        }

        public Spectrum Sample_Le(in Point2D u1, in Point2D u2, out Ray ray, out Normal nLight, out float pdfPos, out float pdfDir)
        {
            ray = new Ray(_pLight, Sampling.Utilities.UniformSampleSphere(u1));
            nLight = (Normal) ray.Direction;
            pdfPos = 1f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
            return I;
        }

        public Spectrum Power() => 4 * System.MathF.PI * I;

        public void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            pdfPos = 0f;
            pdfDir = Sampling.Utilities.UniformSpherePdf();
        }

        public float Pdf_Li(Interaction i, in Vector wi) => 0f;
    }

    public class VisibilityTester
    {
        public Interaction P0 { get; }
        public Interaction P1 { get; }

        public VisibilityTester(Interaction p0, Interaction p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public bool Unoccluded(IScene scene)
        {
            return scene.IntersectP(P0.SpawnRayTo(P1));
        }
    }

    public enum LightType
    {
        Unknown = 0,
        DeltaPosition = 1,
        DeltaDirection = 2,
        Area = 4,
        Infinite = 8
    }
}