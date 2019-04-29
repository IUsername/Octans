﻿namespace Octans.Light
{
    public sealed class DistantLight : ILight
    {
        private readonly Spectrum _l;
        private readonly Vector _wLight;
        private Point _worldCenter;
        private float _worldRadius;

        public DistantLight(Transform lightToWorld, in Spectrum L, in Vector wLight)
        {
            _l = L;
            _wLight = (lightToWorld * wLight).Normalize();
        }

        public void Preprocess(IScene scene)
        {
            scene.WorldBounds.BoundingSphere(out _worldCenter, out _worldRadius);
        }

        public LightType Type => LightType.DeltaDirection;
        public Spectrum Le(in RayDifferential ray) => Spectrum.Zero;

        public Spectrum Sample_Li(Interaction it,
                                  Point2D u,
                                  out Vector wi,
                                  out float pdf,
                                  out VisibilityTester visibility)
        {
            wi = _wLight;
            pdf = 1f;
            var pOutside = it.P + _wLight * (2f * _worldRadius);
            visibility = new VisibilityTester(it, new Interaction(pOutside));
            return _l;
        }

        public Spectrum Sample_Le(in Point2D u1,
                                  in Point2D u2,
                                  out Ray ray,
                                  out Normal nLight,
                                  out float pdfPos,
                                  out float pdfDir)
        {
            var (v1, v2) = MathF.OrthonormalPosZ((Normal) _wLight);
            var cd = Sampling.Utilities.ConcentricSampleDisk(u1);
            var pDisk = _worldCenter + _worldRadius * (cd.X * v1 + cd.Y * v2);
            ray = new Ray(pDisk + _worldRadius * _wLight, -_wLight);
            nLight = (Normal) ray.Direction;
            pdfPos = 1f / (System.MathF.PI * _worldRadius * _worldRadius);
            pdfDir = 1f;
            return _l;
        }

        public Spectrum Power() => _l * (System.MathF.PI * _worldRadius * _worldRadius);

        public void Pdf_Le(Ray ray, Normal nLight, out float pdfPos, out float pdfDir)
        {
            pdfPos = 1f / (System.MathF.PI * _worldRadius * _worldRadius);
            pdfDir = 0f;
        }

        public float Pdf_Li(Interaction i, in Vector wi) => 0f;
    }
}