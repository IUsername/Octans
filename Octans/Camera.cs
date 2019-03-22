using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octans
{
    public partial class Camera
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

        private Color GetColorAtSubPixel(int remaining,
                                         float delta,
                                         ICameraPixelSamples samples,
                                         World w,
                                         in SubPixel sp)
        {
            if (remaining < 1)
            {
                return ColorAtSubPixel(w, in sp);
            }

            var (tl, tr, bl, br) = SubPixel.Corners(in sp);

            var cbr = samples.GetOrAdd(br, SubPixelRenderFunc(w));
            var cbl = samples.GetOrAdd(bl, SubPixelRenderFunc(w));
            var ctr = samples.GetOrAdd(tr, SubPixelRenderFunc(w));
            var ctl = samples.GetOrAdd(tl, SubPixelRenderFunc(w));

            var avg = (ctl + ctr + cbl + cbr) / 4f;

            if (remaining < 2)
            {
                return avg;
            }

            var tlc = IsWithinDelta(in ctl, in avg, delta);
            var trc = IsWithinDelta(in ctr, in avg, delta);
            var blc = IsWithinDelta(in cbl, in avg, delta);
            var brc = IsWithinDelta(in cbr, in avg, delta);

            if (tlc & trc & blc & brc)
            {
                return avg;
            }

            var c = SubPixel.Center(in tl, in br);
            var r = remaining - 1;

            // Increase the necessary differences to sample further.
            var d = delta * 1.2f;
          
            var shared = samples as ISharedCameraPixelSamples;
            if (shared != null)
            {
                samples = shared.CreateLocalScope();
            }

            if (!brc)
            {
                var brCtr = SubPixel.Center(in br, in c);
                cbr = GetColorAtSubPixel(r, d, samples, w, in brCtr);
            }

            if (!blc)
            {
                var blCtr = SubPixel.Center(in bl, in c);
                cbl = GetColorAtSubPixel(r, d, samples, w, in blCtr);
            }

            if (!trc)
            {
                var trCtr = SubPixel.Center(in tr, in c);
                ctr = GetColorAtSubPixel(r, d, samples, w, in trCtr);
            }

            if (!tlc)
            {
                var tlCtr = SubPixel.Center(in tl, in c);
                ctl = GetColorAtSubPixel(r, d, samples, w, in tlCtr);
            }

            shared?.CloseLocalScope(samples);


            return (ctl + ctr + cbl + cbr) / 4f;
        }

        private static SubPixel PixelToCenterSubPixel(int x, int y) => SubPixel.Create(x, y, 2, 1, 1);

        public Canvas Render(World world, int passes = 0, float delta = 0.10f)
        {
            var queue = new ConcurrentQueue<SubPixel>();
            for (var y = 0; y < VSize; y++)
            {
                for (var x = 0; x < HSize; x++)
                {
                    queue.Enqueue(PixelToCenterSubPixel(x, y));
                }
            }
            var samples = new CameraPixelSamples();
            var canvas = new Canvas(HSize, VSize);
            Parallel.ForEach(queue, sp => AdaptiveRender(world, passes, delta, sp, samples, canvas));
            return canvas;
        }

        private void AdaptiveRender(World world,
                                    int passes,
                                    float delta,
                                    in SubPixel sp,
                                    ICameraPixelSamples samples,
                                    Canvas canvas)
        {
            var c = GetColorAtSubPixel(passes, delta, samples, world, in sp);
            canvas.WritePixel(in c, sp.X, sp.Y);
        }

        private static bool IsWithinDelta(in Color a, in Color b, float delta)
        {
            var diff = a - b;
            return MathF.Abs(diff.Red) < delta && MathF.Abs(diff.Green) < delta && MathF.Abs(diff.Blue) < delta;
        }

        private Func<SubPixel, Color> SubPixelRenderFunc(World w)
        {
            return pixel => ColorAtSubPixel(w, in pixel);
        }

        private Color ColorAtSubPixel(World world, in SubPixel sp)
        {
            var dx = (float) sp.Dx / sp.Divisions;
            var dy = (float) sp.Dy / sp.Divisions;
            var xOffset = (sp.X + dx) * PixelSize;
            var yOffset = (sp.Y + dy) * PixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var inv = _transformInverse;
            var pixel = inv * new Point(worldX, worldY, -1f);
            var origin = inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            var r = new Ray(origin, direction);
            return Shading.ColorAt(world, r, 4);
        }
    }
}