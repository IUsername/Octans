using Octans.Primitive;
using static System.MathF;
using static Octans.MathF;
using static Octans.Sampling.Utilities;

namespace Octans.Light
{
    public sealed class DiffuseAreaLight : IAreaLight
    {
        private readonly float _area;
        private readonly Spectrum _lemit;
        private readonly IMedium _mediumInterface;
        private readonly IShape _shape;
        private readonly bool _twoSided;
        private readonly Transform _lightToWorld;
        private readonly Transform _worldToLight;

        public DiffuseAreaLight(Transform lightToWorld,
                                IMedium mediumInterface,
                                Spectrum Lemit,
                                int nSamples,
                                IShape shape,
                                bool twoSided = false)
        {
            _lightToWorld = lightToWorld;
            _mediumInterface = mediumInterface;
            _worldToLight = Transform.Invert(lightToWorld);
            _lemit = Lemit;
            _shape = shape;
            _twoSided = twoSided;
            _area = shape.Area();

            // TODO: Check for scale
        }

        public void Preprocess(IScene scene)
        {
        }

        public LightType Type => LightType.Area;

        public Spectrum Le(in RayDifferential ray) => Spectrum.Zero;

        public Spectrum Sample_Li(Interaction it,
                                  Point2D u,
                                  out Vector wi,
                                  out float pdf,
                                  out VisibilityTester visibility)
        {
            var pShape = _shape.Sample(it, u, out pdf);
            pShape.MediumInterface = _mediumInterface;
            if (pdf == 0f)
            {
                wi = Vectors.Zero;
                visibility = null;
                return Spectrum.Zero;
            }
            var v = pShape.P - it.P;

            if (v.MagSqr() == 0f)
            {
                pdf = 0;
                wi = Vectors.Zero;
                visibility = null;
                return Spectrum.Zero;
            }

            wi = v.Normalize();
            visibility = new VisibilityTester(it, pShape);
            return L(pShape, -wi);
        }

        public Spectrum Sample_Le(in Point2D u1,
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

        public Spectrum Power() => (_twoSided ? 2f : 1f) * _lemit * _area * PI;

        public void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            var it = new Interaction();
            it.Initialize(ray.Origin, nLight, Vectors.Zero, (Vector) nLight, _mediumInterface);
            pdfPos = _shape.Pdf(it);
            pdfDir = _twoSided
                ? 0.5f * CosineHemispherePdf(Abs(nLight % ray.Direction))
                : CosineHemispherePdf(nLight % ray.Direction);
        }

        public float Pdf_Li(Interaction i, in Vector wi) => _shape.Pdf(i, wi);

        public Spectrum L(Interaction intr, in Vector w) =>
            _twoSided || (intr.N % w) > 0f ? _lemit : Spectrum.Zero;
    }
}