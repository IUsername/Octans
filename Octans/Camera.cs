using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octans
{
    public class LocatedPinholeCamera : IPixelRenderer
    {
        private readonly World _world;
        private readonly Matrix _transformInverse;
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly float _pixelSize;

        public LocatedPinholeCamera(World world, in Matrix transform, float fieldOfView, int hSize, int vSize)
        {
            _world = world;
            _transformInverse = transform.Inverse();

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

        public Color Render(in SubPixel sp)
        {
            return ColorAtSubPixel(_world, in sp);
        }

        public Ray PixelToRay(in SubPixel sp)
        {
            var dx = (float)sp.Dx / sp.Divisions;
            var dy = (float)sp.Dy / sp.Divisions;
            var xOffset = (sp.X + dx) * _pixelSize;
            var yOffset = (sp.Y + dy) * _pixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var inv = _transformInverse;
            var pixel = inv * new Point(worldX, worldY, -1f);
            var origin = inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            return new Ray(origin, direction);
        }

        private Color ColorAtSubPixel(World world, in SubPixel sp)
        {
            var r = PixelToRay(in sp);
            return Shading.ColorAt(world, in r, 4);
        }
    }

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

        public Canvas Render(World world, int maxPasses = 0, float tolerance = 0.10f)
        {
            var queue = new ConcurrentQueue<(int x, int y)>();
            for (var y = 0; y < VSize; y++)
            {
                for (var x = 0; x < HSize; x++)
                {
                    queue.Enqueue((x,y));
                }
            }

            var canvas = new Canvas(HSize, VSize);
            var located = new LocatedPinholeCamera(world, Transform, FieldOfView, HSize, VSize);
            var adaptive = new AdaptiveRenderer(maxPasses, tolerance, located);
            Parallel.ForEach(queue, p => RenderToCanvas(p.x, p.y, canvas, adaptive));
            return canvas;
        }

        private static void RenderToCanvas(int x, int y, Canvas canvas, IPixelRenderer renderer)
        {
            var sp = SubPixel.ForPixelCenter(x, y);
            var c = renderer.Render(in sp);
            canvas.WritePixel(in c, x, y);
        }
    }
}