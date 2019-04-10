using System;

namespace Octans.Camera
{
    public class PinholeCamera : ICameraSampler
    {
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly float _width;
        private readonly Point _origin;

        public PinholeCamera(in Transform transform, float fieldOfView, float aspectRatio)
        {
            CameraToWorld = Transform.Invert(transform);
            FieldOfView = fieldOfView;
            AspectRatio = aspectRatio;
           // _inv = transform.Inverse();
            var halfView = MathF.Tan(fieldOfView / 2f);
            var aspect = aspectRatio;
            var halfWidth = halfView;
            var halfHeight = halfView;
            if (aspect >= 1f)
            {
                halfHeight = halfView / aspect;
            }
            else
            {
                halfWidth = halfView * aspect;
            }

            _halfHeight = halfHeight;
            _halfWidth = halfWidth;
            _width = halfWidth * 2f;
            _origin = CameraToWorld * Point.Zero;
        }

        public Transform CameraToWorld { get; }
        public float FieldOfView { get; }
        public float AspectRatio { get; }

        public (Ray ray, float throughput) CameraRay(in PixelSample sample, ISampler sampler)
        {
            var pixelSize = _width / sample.Pixel.Width;
            var xOffset = (sample.Pixel.Coordinate.X + sample.U) * pixelSize;
            var yOffset = (sample.Pixel.Coordinate.Y + sample.V) * pixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var pixel = CameraToWorld * new Point(worldX, worldY, -1f);
            var direction = (pixel - _origin).Normalize();
            return (new Ray(_origin, direction), 1f);
        }
    }
}