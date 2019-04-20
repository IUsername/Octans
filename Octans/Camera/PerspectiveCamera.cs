using static System.MathF;
using static Octans.Sampling.Utilities;
using static Octans.Transform;

namespace Octans.Camera
{
    public sealed class PerspectiveCamera : ProjectiveCamera
    {
        private readonly Vector _dxCamera;
        private readonly Vector _dyCamera;

        public PerspectiveCamera(Transform cameraToWorld,
                                 Bounds2D screenWindow,
                                 float lensRadius,
                                 float focalDistance,
                                 float fov,
                                 Film film)
            : base(cameraToWorld, Perspective(fov, 1e-2f, 1000f), screenWindow, lensRadius, focalDistance, film)
        {
            _dxCamera = RasterToCamera * new Point(1, 0, 0) - RasterToCamera * new Point(0, 0, 0);
            _dyCamera = RasterToCamera * new Point(0, 1, 0) - RasterToCamera * new Point(0, 0, 0);

            var res = film.FullResolution;
            var pMin = RasterToCamera * new Point(0, 0, 0);
            var pMax = RasterToCamera * new Point(res.X, res.Y, 0);
            pMin /= pMin.Z;
            pMax /= pMax.Z;
            A = Abs((pMax.X - pMin.X) * (pMax.Y - pMin.Y));
        }

        private double A { get; }


        public override float GenerateRayDifferential(CameraSample cameraSample, out Ray ray)
        {
            var pFilm = new Point(cameraSample.FilmPoint.X, cameraSample.FilmPoint.Y, 0);
            var pCamera = RasterToCamera * pFilm;
            var r = new Ray(new Point(), ((Vector) pCamera).Normalize());
            if (LensRadius > 0f)
            {
                var pLens = LensRadius * ConcentricSampleDisk(cameraSample.LensPoint);

                var ft = FocalDistance / r.Direction.Z;
                var pFocus = r.Position(ft);
                var o = new Point(pLens.X, pLens.Y, 0);
                r = new Ray(o, (pFocus - o).Normalize());
            }

            ray = CameraToWorld * r;
            return 1f;
        }

        public static PerspectiveCamera Create(Transform cameraToWorld,
                                               float aspectRatio,
                                               float lensRadius,
                                               float focalDistance,
                                               float fov,
                                               Film film)
        {
            var screen = aspectRatio > 1f 
                ? new Bounds2D(-aspectRatio, -1f, aspectRatio, 1f) 
                : new Bounds2D(-1f, -1f/aspectRatio, 1f, 1f/aspectRatio);

            return new PerspectiveCamera(cameraToWorld, screen, lensRadius, focalDistance, fov, film);
        }
    }
}