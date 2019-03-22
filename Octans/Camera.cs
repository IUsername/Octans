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

        private Color GetColorAtSubPixel(int remaining,
                                         float delta,
                                         ConcurrentDictionary<SubPixel, Color> samples,
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
            var d = delta * 4f;
            // TODO: Create non-concurrent inner pixel sample lookup.
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

            return (ctl + ctr + cbl + cbr) / 4f;
        }

        private static SubPixel PixelToCenterSubPixel(int x, int y) => SubPixel.Create(x, y, 2, 1, 1);

        public Canvas Render(World world, int passes = 0, float delta = 0.10f)
        {
            var samples = new ConcurrentDictionary<SubPixel, Color>();
            var queue = new ConcurrentQueue<SubPixel>();
            for (var y = 0; y < VSize; y++)
            {
                for (var x = 0; x < HSize; x++)
                {
                    queue.Enqueue(PixelToCenterSubPixel(x, y));
                }
            }

            var canvas = new Canvas(HSize, VSize);
            Parallel.ForEach(queue, sp => AdaptiveRender(world, passes, delta, sp, samples, canvas));
            return canvas;
        }

        private void AdaptiveRender(World world,
                                    int passes,
                                    float delta,
                                    in SubPixel sp,
                                    ConcurrentDictionary<SubPixel, Color> samples,
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

        public readonly struct SubPixel : IEquatable<SubPixel>
        {
            public int X { get; }
            public int Y { get; }
            public int Divisions { get; }
            public int Dx { get; }
            public int Dy { get; }

            public SubPixel(int x, int y, int divisions, int dx, int dy)
            {
                X = x;
                Y = y;
                Divisions = divisions;
                Dx = dx;
                Dy = dy;
            }

            public bool Equals(SubPixel other) => X == other.X && Y == other.Y && Divisions == other.Divisions &&
                                                  Dx == other.Dx && Dy == other.Dy;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is SubPixel other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = X;
                    hashCode = (hashCode * 397) ^ Y;
                    hashCode = (hashCode * 397) ^ Divisions.GetHashCode();
                    hashCode = (hashCode * 397) ^ Dx.GetHashCode();
                    hashCode = (hashCode * 397) ^ Dy.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(SubPixel left, SubPixel right) => left.Equals(right);

            public static bool operator !=(SubPixel left, SubPixel right) => !left.Equals(right);

            public static (SubPixel tl, SubPixel tr, SubPixel bl, SubPixel br) Corners(in SubPixel center)
            {
                var x = center.X;
                var y = center.Y;
                var div = center.Divisions;
                var dx = center.Dx;
                var dy = center.Dy;
                var tl = Create(x, y, div, dx - 1, dy - 1);
                var tr = Create(x, y, div, dx + 1, dy - 1);
                var bl = Create(x, y, div, dx - 1, dy + 1);
                var br = Create(x, y, div, dx + 1, dy + 1);
                return (tl, tr, bl, br);
            }

            public static SubPixel Create(int x, int y, int divisions, int dx, int dy)
            {
                if (dx == divisions)
                {
                    x += 1;
                    dx = 0;
                }

                if (dy == divisions)
                {
                    y += 1;
                    dy = 0;
                }

                while (divisions > 1 && dx % 2 == 0 && dy % 2 == 0)
                {
                    dx >>= 1;
                    dy >>= 1;
                    divisions >>= 1;
                }

                return new SubPixel(x, y, divisions, dx, dy);
            }

            private static SubPixel ToDivResolution(in SubPixel sp, int resolution)
            {
                if (sp.Divisions == resolution)
                {
                    return sp;
                }

                var fac = resolution / sp.Divisions;
                return new SubPixel(sp.X, sp.Y, resolution, sp.Dx * fac, sp.Dy * fac);
            }

            public static SubPixel Center(in SubPixel a, in SubPixel b)
            {
                var div = Math.Max(a.Divisions << 1, b.Divisions << 1);
                var utl = ToDivResolution(in a, div);
                var ubr = ToDivResolution(in b, div);
                var xl = utl.X * div + utl.Dx;
                var xr = ubr.X * div + ubr.Dx;
                var yt = utl.Y * div + utl.Dy;
                var yb = ubr.Y * div + ubr.Dy;
                var dx = Math.Min(xl, xr) + Math.Abs(xr - xl) / 2;
                var dy = Math.Min(yt, yb) + Math.Abs(yb - yt) / 2;
                var x = dx / div;
                dx -= x * div;
                var y = dy / div;
                dy -= y * div;
                return Create(x, y, div, dx, dy);
            }
        }
    }
}