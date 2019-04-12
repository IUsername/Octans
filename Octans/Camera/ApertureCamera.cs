using System;
using Octans.Sampling;

namespace Octans.Camera
{
    public class ApertureCamera : ICameraSampler
    {
        private readonly Point _camera;
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly float _width;

        public ApertureCamera(float fieldOfView,
                              float aspectRatio,
                              float apertureRadius,
                              Point from,
                              Point to,
                              Vector up,
                              float focalDistance = -1)
        {
            ApertureRadius = apertureRadius;
            CameraToWorld = Transform.Invert(Transform.LookAt(from, to, up));

            var halfView = MathF.Tan(fieldOfView / 2f);
            var halfWidth = halfView;
            var halfHeight = halfView;
            if (aspectRatio >= 1f)
            {
                halfHeight = halfView / aspectRatio;
            }
            else
            {
                halfWidth = halfView * aspectRatio;
            }

            _halfHeight = halfHeight;
            _halfWidth = halfWidth;
            FocalDistance = focalDistance < 0 ? Point.Distance(from, to) : focalDistance;

            _width = halfWidth * 2f;
            _camera = CameraToWorld * Point.Zero;
        }

        public float FocalDistance { get; }

        public Transform CameraToWorld { get; }

        public float ApertureRadius { get; }

        public (Ray ray, float throughput) CameraRay(in PixelSample sample, ISampler sampler)
        {
            var uv = sampler.NextUV();
            var r = MathF.Sqrt(1f - uv.U * uv.U);
            var phi = 2 * MathF.PI * uv.V;
            var u = ApertureRadius * MathF.Cos(phi) * r;
            var v = ApertureRadius * MathF.Sin(phi) * r;
            var offset = new Point(u, v, 0);

            var aimed = FocalPlanePoint(in sample);

            var start = CameraToWorld * offset;
            var dir = (aimed - start).Normalize();
            var ray = new Ray(start, dir);
            return (ray, 1f);
        }


        private Point FocalPlanePoint(in PixelSample sample)
        {
            var pixelSize = _width / sample.Pixel.Width;
            var xOffset = (sample.Pixel.Coordinate.X + sample.UV.U) * pixelSize;
            var yOffset = (sample.Pixel.Coordinate.Y + sample.UV.V) * pixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var pixel = CameraToWorld * new Point(worldX, worldY, -1f);
            var direction = (pixel - _camera).Normalize();
            var r = new Ray(_camera, direction);
            return r.Position(FocalDistance);
        }
    }
}