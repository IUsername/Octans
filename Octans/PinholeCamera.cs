using System;

namespace Octans
{
    public class PinholeCamera : ICamera
    {
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly Matrix _inv;
        public float PixelSize { get; }

        public PinholeCamera(in Matrix transform, float fieldOfView, int hSize, int vSize)
        {
            Transform = transform;
            FieldOfView = fieldOfView;
            HSize = hSize;
            VSize = vSize;
            _inv = transform.Inverse();
            var halfView = MathF.Tan(fieldOfView / 2f);
            var aspect = (float) hSize / vSize;
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
            PixelSize = halfWidth * 2f / hSize;
        }

        public Matrix Transform { get; }
        public float FieldOfView { get; }
        public int HSize { get; }
        public int VSize { get; }

        public Color Render(IScene scene, in SubPixel sp)
        {
            var r = PixelToRay(in sp);
            return scene.World.ColorFor(in r);
        }

        public Ray PixelToRay(in SubPixel sp)
        {
            var dx = (float) sp.Dx / sp.Divisions;
            var dy = (float) sp.Dy / sp.Divisions;
            var xOffset = (sp.X + dx) * PixelSize;
            var yOffset = (sp.Y + dy) * PixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var pixel = _inv * new Point(worldX, worldY, -1f);
            var origin = _inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            return new Ray(origin, direction);
        }
    }
}