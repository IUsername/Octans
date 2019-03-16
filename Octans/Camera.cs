using System;

namespace Octans
{
    public class Camera
    {
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private Matrix _transformInverse;

        public Camera(int hSize, int vSize, float fieldOfView)
        {
            HSize = hSize;
            VSize = vSize;
            FieldOfView = fieldOfView;
            Transform = Matrix.Identity;
            _transformInverse = Matrix.Identity;

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

        public int HSize { get; }
        public int VSize { get; }
        public float FieldOfView { get; }
        public Matrix Transform { get; private set; }
        public float PixelSize { get; }

        public void SetTransform(in Matrix transform)
        {
            Transform = transform;
            _transformInverse = transform.Inverse();
        }

        public Ray RayForPixel(int px, int py)
        {
            var xOffset = (px + 0.5f) * PixelSize;
            var yOffset = (py + 0.5f) * PixelSize;
            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var inv = _transformInverse;
            var pixel = inv * new Point(worldX, worldY, -1f);
            var origin = inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            return new Ray(origin, direction);
        }

        public Canvas Render(World world)
        {
            var canvas = new Canvas(HSize, VSize);
            for (var y = 0; y < VSize; y++)
            {
                for (var x = 0; x < HSize; x++)
                {
                    var ray = RayForPixel(x, y);
                    var color = Shading.ColorAt(world, ray, 8);
                    canvas.WritePixel(color, x, y);
                }
            }
            return canvas;
        }
    }
}