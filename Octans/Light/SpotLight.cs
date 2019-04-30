using static System.MathF;
using static Octans.MathF;

namespace Octans.Light
{
    public sealed class SpotLight : Light
    {
        private readonly float _cosFalloffStart;
        private readonly float _cosTotalWidth;
        private readonly Spectrum _I;
        private readonly Point _pLight;

        public SpotLight(Transform lightToWorld, IMedium m, Spectrum I, float totalWidth, float falloffStart)
            : base(LightType.DeltaDirection, lightToWorld, m)
        {
            _I = I;
            _pLight = lightToWorld * Point.Zero;
            _cosTotalWidth = Cos(Rad(totalWidth));
            _cosFalloffStart = Cos(Rad(falloffStart));
        }

        public override Spectrum Sample_Li(Interaction reference,
                                           Point2D u,
                                           out Vector wi,
                                           out float pdf,
                                           out VisibilityTester visibility)
        {
            wi = (_pLight - reference.P).Normalize();
            pdf = 1f;
            visibility = new VisibilityTester(reference, new Interaction(_pLight, MediumInterface));
            return _I * Falloff(-wi) / Point.DistanceSqr(_pLight, reference.P);
        }

        public override Spectrum Sample_Le(in Point2D u1,
                                           in Point2D u2,
                                           out Ray ray,
                                           out Normal nLight,
                                           out float pdfPos,
                                           out float pdfDir)
        {
            var w = Sampling.Utilities.UniformSampleCone(u1, _cosTotalWidth);
            // TODO: Medium
            ray = new Ray(_pLight, LightToWorld * w);
            nLight = (Normal) ray.Direction;
            pdfPos = 1f;
            pdfDir = Sampling.Utilities.UniformConePdf(_cosTotalWidth);
            return _I * Falloff(ray.Direction);
        }

        private float Falloff(in Vector w)
        {
            var wl = (WorldToLight * w).Normalize();
            var cosTheta = wl.Z;
            if (cosTheta < _cosTotalWidth)
            {
                return 0f;
            }

            if (cosTheta >= _cosFalloffStart)
            {
                return 1f;
            }

            var delta = (cosTheta - _cosTotalWidth) / (_cosFalloffStart - _cosTotalWidth);
            return delta * delta * (delta * delta);
        }

        public override Spectrum Power() => _I * 2f * PI * (1f - 0.5f * (_cosFalloffStart + _cosTotalWidth));

        public override void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            pdfPos = 0f;
            pdfDir = CosTheta(WorldToLight * ray.Direction) >= _cosTotalWidth
                ? Sampling.Utilities.UniformConePdf(_cosTotalWidth)
                : 0f;
        }

        public override float Pdf_Li(Interaction i, in Vector wi) => 0f;
    }
}