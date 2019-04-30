using Octans.Primitive;
using static System.MathF;
using static Octans.MathF;
using static Octans.Sampling.Utilities;

namespace Octans.Light
{
    public sealed class DiffuseAreaLight : AreaLight
    {
        private readonly float _area;
        private readonly Spectrum _lemit;
        private readonly IMedium _mediumInterface;
        private readonly IShape _shape;
        private readonly bool _twoSided;

        public DiffuseAreaLight(Transform lightToWorld,
                                IMedium mediumInterface,
                                Spectrum Lemit,
                                int nSamples,
                                IShape shape,
                                bool twoSided = false)
            : base(lightToWorld, mediumInterface, nSamples)
        {
            _mediumInterface = mediumInterface;
            _lemit = Lemit;
            _shape = shape;
            _twoSided = twoSided;
            _area = shape.Area();

            // TODO: Check for scale
        }

        public Spectrum Le(in RayDifferential ray) => Spectrum.Zero;

        public override Spectrum Sample_Li(Interaction reference,
                                           Point2D u,
                                           out Vector wi,
                                           out float pdf,
                                           out VisibilityTester visibility)
        {
            var pShape = _shape.Sample(reference, u, out pdf);
            pShape.MediumInterface = _mediumInterface;
            if (pdf == 0f)
            {
                wi = Vectors.Zero;
                visibility = null;
                return Spectrum.Zero;
            }

            var v = pShape.P - reference.P;

            if (v.LengthSquared() == 0f)
            {
                pdf = 0;
                wi = Vectors.Zero;
                visibility = null;
                return Spectrum.Zero;
            }

            wi = v.Normalize();
            visibility = new VisibilityTester(reference, pShape);
            return L(pShape, -wi);
        }

        public override Spectrum Sample_Le(in Point2D u1,
                                           in Point2D u2,
                                           out Ray ray,
                                           out Normal nLight,
                                           out float pdfPos,
                                           out float pdfDir)
        {
            var pShape = _shape.Sample(u1, out pdfPos);
            pShape.MediumInterface = _mediumInterface;
            nLight = pShape.N;

            Vector w;
            if (_twoSided)
            {
                var u = u2;
                if (u[0] < 0.5f)
                {
                    u = new Point2D(Min(u[0] * 2f, OneMinusEpsilon), u.Y);
                    w = CosineSampleHemisphere(u);
                }
                else
                {
                    u = new Point2D(Min((u[0] - 0.5f) * 2f, OneMinusEpsilon), u.Y);
                    w = CosineSampleHemisphere(u);
                    w = new Vector(w.X, w.Y, -w.Z);
                }

                pdfDir = 0.5f * CosineHemispherePdf(Abs(w.Z));
            }
            else
            {
                w = CosineSampleHemisphere(u2);
                pdfDir = CosineHemispherePdf(w.Z);
            }

            var (v1, v2) = OrthonormalPosZ(pShape.N);
            w = w.X * v1 + w.Y * v2 + w.Z * (Vector) pShape.N;
            ray = pShape.SpawnRay(w);
            return L(pShape, w);
        }

        public override Spectrum Power() => (_twoSided ? 2f : 1f) * _lemit * _area * PI;

        public override void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            var it = new Interaction();
            it.Initialize(ray.Origin, nLight, Vectors.Zero, (Vector) nLight, _mediumInterface);
            pdfPos = _shape.Pdf(it);
            pdfDir = _twoSided
                ? 0.5f * CosineHemispherePdf(Abs(nLight % ray.Direction))
                : CosineHemispherePdf(nLight % ray.Direction);
        }

        public override float Pdf_Li(Interaction i, in Vector wi) => _shape.Pdf(i, wi);

        public override Spectrum L(Interaction intr, in Vector w) =>
            _twoSided || intr.N % w > 0f ? _lemit : Spectrum.Zero;
    }
}