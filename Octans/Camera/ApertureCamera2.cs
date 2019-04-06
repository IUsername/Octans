using System;

namespace Octans.Camera
{
    public class ApertureCamera2 : ICameraSampler
    {
        private readonly float _focalDist;
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly Matrix _inv;
        private readonly float _width;
        private readonly float _height;
        private readonly Point _camera;

        public ApertureCamera2(float fieldOfView,
                               float aspectRatio, 
                               float apertureRadius,
                               Point from,
                               Point to,
                               float focalDistance = -1)
        {
            ApertureRadius = apertureRadius;
            var transform = Transforms.View(@from, to, new Vector(0, 1, 0));
            _inv = transform.Inverse();
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
            _focalDist = focalDistance < 0 ? (@from - to).Magnitude() : focalDistance;

            _width = halfWidth * 2f;
            _height = halfHeight * 2f;

            _camera = _inv * Point.Zero;
        }

        public float ApertureRadius { get; }


        private Point FocalPlanePoint(double u, double v)
        {
            var xOffset = u*_width;
            var yOffset = v*_height;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var pixel = _inv * new Point((float) worldX, (float)worldY, -1f);
            var direction = (pixel - _camera).Normalize();
            var r = new Ray(_camera, direction);
            return r.Position(_focalDist);
        }

        public (Ray ray, float throughput) CameraRay(in PixelSample sample, ISampler sampler)
        {
            var (u1, u2) = sampler.NextUV();
            var r = MathF.Sqrt(1f - u1 * u1);
            var phi = 2 * MathF.PI * u2;
            var u = ApertureRadius * MathF.Cos(phi) * r;
            var v = ApertureRadius * MathF.Sin(phi) * r;
            var offset = new Vector(u, v, 0);

            var aimed = FocalPlanePoint(sample.U, sample.V);

            var start = _inv * (Point.Zero + offset);
            var dir = (aimed - start).Normalize();
            var ray = new Ray(start, dir);
            return (ray, 1f);
        }
    }
}