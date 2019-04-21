using static Octans.Transform;

namespace Octans.Camera
{
    public abstract class ProjectiveCamera : ICamera
    {
        protected ProjectiveCamera(
            Transform cameraToWorld,
            Transform cameraToScreen,
            Bounds2D screenWindow,
            float lensRadius,
            float focalDistance,
            Film film)
        {
            CameraToWorld = cameraToWorld;
            CameraToScreen = cameraToScreen;
            LensRadius = lensRadius;
            FocalDistance = focalDistance;
            Film = film;

            ScreenToRaster = Scale(film.FullResolution.X, film.FullResolution.Y, 1f) *
                             Scale(1f / (screenWindow.Max.X - screenWindow.Min.X),
                                   1f / (screenWindow.Min.Y - screenWindow.Max.Y),
                                   1f) *
                             Translate(-screenWindow.Min.X, -screenWindow.Max.Y, 0f);
            RasterToScreen = Invert(ScreenToRaster);
            RasterToCamera = Invert(CameraToScreen) * RasterToScreen;
        }

        public Transform CameraToWorld { get; }

        public Transform CameraToScreen { get; }

        protected float LensRadius { get; }

        protected float FocalDistance { get; }

        protected Transform RasterToCamera { get; }

        protected Transform RasterToScreen { get; }

        protected Transform ScreenToRaster { get; }

        public Film Film { get; }
        public abstract float GenerateRayDifferential(in CameraSample sample, out RayDifferential ray);
        public abstract float GenerateRay(in CameraSample sample, out Ray ray);
    }
}