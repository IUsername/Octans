using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
            var worldShading = new RaytracedWorld(4, world);
            //var camera = new PinholeCamera(Transform, FieldOfView, HSize, VSize);
            var camera = new ApertureCamera(FieldOfView, HSize, VSize, 0.05f, new Point(0, 1.25f, -4f), new Point(0, 1, 0), 3.5f);
            var scene = new Scene(camera, worldShading);
            var adaptive = new AdaptiveRenderer(maxPasses, tolerance, scene);
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