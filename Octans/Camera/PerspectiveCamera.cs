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


        public override float GenerateRayDifferential(in CameraSample sample, IObjectArena arena, out RayDifferential ray)
        {
            var pFilm = new Point(sample.FilmPoint.X, sample.FilmPoint.Y, 0);
            var pCamera = RasterToCamera * pFilm;
            var dir = ((Vector) pCamera).Normalize();
            var r =  arena.Create<RayDifferential>().Initialize(Point.Zero, dir);
            if (LensRadius > 0f)
            {
                var pLens = LensRadius * ConcentricSampleDisk(sample.LensPoint);

                var ft = FocalDistance / r.Direction.Z;
                var pFocus = r.Position(ft);
                var o = new Point(pLens.X, pLens.Y, 0);
                r = new RayDifferential(o, (pFocus - o).Normalize());

                var dx = ((Vector) (pCamera + _dxCamera)).Normalize();
                ft = FocalDistance / dx.Z;
                pFocus = new Point(0, 0, 0) + ft * dx;
                r.RxOrigin = new Point(pLens.X, pLens.Y, 0f);
                r.RxDirection = (pFocus - r.RxOrigin).Normalize();

                var dy = ((Vector) (pCamera + _dyCamera)).Normalize();
                ft = FocalDistance / dy.Z;
                pFocus = new Point(0, 0, 0) + ft * dy;
                r.RyOrigin = new Point(pLens.X, pLens.Y, 0f);
                r.RyDirection = (pFocus - r.RyOrigin).Normalize();
            }
            else
            {
                r.RxOrigin = r.Origin;
                r.RyOrigin = r.Origin;
                r.RxDirection = ((Vector) pCamera + _dxCamera).Normalize();
                r.RyDirection = ((Vector) pCamera + _dyCamera).Normalize();
            }

            r.HasDifferentials = true;
            ApplyInPlace(CameraToWorld, r);
            ray = r;
            return 1f;
        }

        public override float GenerateRay(in CameraSample sample, out Ray ray)
        {
            var pFilm = new Point(sample.FilmPoint.X, sample.FilmPoint.Y, 0);
            var pCamera = RasterToCamera * pFilm;
            var r = new Ray(new Point(), ((Vector) pCamera).Normalize());
            if (LensRadius > 0f)
            {
                var pLens = LensRadius * ConcentricSampleDisk(sample.LensPoint);

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
                : new Bounds2D(-1f, -1f / aspectRatio, 1f, 1f / aspectRatio);

            return new PerspectiveCamera(cameraToWorld, screen, lensRadius, focalDistance, fov, film);
        }
    }
}