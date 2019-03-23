using System;

namespace Octans
{
    public class PinholeCamera : ICamera
    {
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly float _pixelSize;

        public PinholeCamera(float fieldOfView, int hSize, int vSize)
        {
            var halfView = MathF.Tan(fieldOfView / 2f);
            var aspect = (float)hSize / vSize;
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
            _pixelSize = halfWidth * 2f / hSize;
        }

        private Ray PixelToRay(in IScene scene, in SubPixel sp)
        {
            var dx = (float)sp.Dx / sp.Divisions;
            var dy = (float)sp.Dy / sp.Divisions;
            var xOffset = (sp.X + dx) * _pixelSize;
            var yOffset = (sp.Y + dy) * _pixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var inv = scene.CameraPosition.TransformInverse;
            var pixel = inv * new Point(worldX, worldY, -1f);
            var origin = inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            return new Ray(origin, direction);
        }
        
        public Color Render(IScene scene, in SubPixel sp)
        {
            var r = PixelToRay(in scene, in sp);
            return scene.World.ColorFor(in r);
        }
    }
}