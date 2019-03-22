using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

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

        //public Canvas Render(World world)
        //{
        //    var canvas = new Canvas(HSize, VSize);
        //    for (var y = 0; y < VSize; y++)
        //    {
        //        for (var x = 0; x < HSize; x++)
        //        {
        //            RenderPixel(world, x, y, canvas);
        //        }
        //    }
        //    return canvas;
        //}

        public Canvas Render(World world)
        {
            var canvas = new Canvas(HSize, VSize);
            Parallel.For(0, VSize, y => RenderRow(world, y, canvas));
            return canvas;
        }

        private Color GetColorAtSubPixel(int remaining, float delta, ConcurrentDictionary<SubPixel, Color> samples, World w, in SubPixel sp)
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

            if (!brc)
            {
                var brCtr = SubPixel.Center(in br, in c);
                cbr = GetColorAtSubPixel(r, delta, samples, w, in brCtr);
            }

            if (!blc)
            {
                var blCtr = SubPixel.Center(in bl, in c);
                cbl = GetColorAtSubPixel(r, delta, samples, w, in blCtr);
            }
            if (!trc)
            {
                var trCtr = SubPixel.Center(in tr, in c);
                ctr = GetColorAtSubPixel(r, delta, samples, w, in trCtr);
            }
            if (!tlc)
            {
                var tlCtr = SubPixel.Center(in tl, in c);
                ctl = GetColorAtSubPixel(r, delta, samples, w, in tlCtr);
            }
            return (ctl + ctr + cbl + cbr) / 4f;
        }

        private static SubPixel PixelToCenterSubPixel(int x, int y)
        {
            return SubPixel.Create(x,y,2,1,1);
        }

        public Canvas RenderAAA2(World world, int passes = 3, float delta = 0.15f)
        {
            var samples = new ConcurrentDictionary<SubPixel, Color>();
            var canvas = new Canvas(HSize, VSize);
            var queue = new ConcurrentQueue<SubPixel>();

            //Parallel.For(0, VSize + 1, y => RenderSubPixel2(y, world, samples, queue));

            var list = new List<SubPixel>();
                for (var y = 0; y < VSize; y++)
                {
                    for (var x = 0; x < HSize; x++)
                    {
                        var sp = PixelToCenterSubPixel(x, y);
                        list.Add(sp);
                    //    queue.Enqueue(sp);
                    }

                }

                var skip = 107;
                for (int i = 0; i < skip; i++)
                {
                    for (int index = i; index < list.Count; index += skip)
                    {
                        queue.Enqueue(list[index]);
                    }
                }
           
                             

          

            Parallel.ForEach(queue, sp => AdaptiveRender(world, passes, delta, sp, samples, canvas));

            return canvas;
        }

        private void RenderSubPixel2(int y, World world, ConcurrentDictionary<SubPixel, Color> samples, ConcurrentQueue<SubPixel> queue)
        {
            for (var x = 0; x <= HSize; x++)
            {
                var sp = new SubPixel(x, y, 1, 0, 0);
                var color = ColorAtSubPixel(world, sp);
                samples.TryAdd(sp, color);
                if (x < HSize && y < VSize)
                {
                    queue.Enqueue(SubPixel.Create(x, y, 2, 1, 1));
                }
            }
        }

        private void AdaptiveRender(World world, int passes, float delta, in SubPixel sp, ConcurrentDictionary<SubPixel, Color> samples, Canvas canvas)
        {
            var c = GetColorAtSubPixel(passes, delta, samples, world, in sp);
            canvas.WritePixel(in c, sp.X, sp.Y);
        }

        public Canvas RenderAAA(World world)
        {
            var samples = new ConcurrentDictionary<SubPixel, Color>();
            Parallel.For(0, VSize + 1, y => RenderSubPixel(y, world, samples));

            var canvas = new Canvas(HSize, VSize);
            var queue = new ConcurrentQueue<SubPixel>();
            Parallel.For(0, VSize, y =>
            {
                var delta = 0.05f;
                for (var x = 0; x < HSize; x++)
                {
                    var sp1 = new SubPixel(x, y, 1, 0, 0);
                    var sp2 = new SubPixel(x+1, y, 1, 0, 0);
                    var sp3 = new SubPixel(x, y+1, 1, 0, 0);
                    var sp4 = new SubPixel(x+1, y+1, 1, 0, 0);
                   
                    var color1 = samples.GetValueOrDefault(sp1, Colors.Magenta);
                    var color2 = samples.GetValueOrDefault(sp2, Colors.Magenta);
                    var color3 = samples.GetValueOrDefault(sp3, Colors.Magenta);
                    var color4 = samples.GetValueOrDefault(sp4, Colors.Magenta);
                    var avg = (color1 + color2 + color3 + color4) / 4f;

                    var tl = IsWithinDelta(in color1, in avg, delta);
                    var tr = IsWithinDelta(in color2, in avg, delta);
                    var bl = IsWithinDelta(in color3, in avg, delta);
                    var br = IsWithinDelta(in color4, in avg, delta);

                    if (tl & tr & bl & br)
                    {
                        canvas.WritePixel(in avg, x, y);
                    }
                    else
                    {
                        //canvas.WritePixel(in Colors.Magenta, x, y);
                        canvas.WritePixel(in avg, x, y);
                        var c = new SubPixel(x,y,2,1,1);
                        queue.Enqueue(c);
                    }
                }
            });

            Parallel.ForEach(queue, sp => SecondLevel(world, sp, samples, canvas));

            return canvas;
        }

        private void SecondLevel(World world, SubPixel sp, ConcurrentDictionary<SubPixel, Color> samples, Canvas canvas)
        {
            var color = ColorAtSubPixel(world, sp);
            samples.TryAdd(sp, color);
            var prior = canvas.PixelAt(sp.X, sp.Y);
            var avg = (prior + color) / 2f;
            canvas.WritePixel(in avg, sp.X, sp.Y);
        }

        private static bool IsWithinDelta(in Color a, in Color b, float delta)
        {
            var diff = a - b;
            return MathF.Abs(diff.Red) < delta && MathF.Abs(diff.Green) < delta && MathF.Abs(diff.Blue) < delta;
        }

        private void RenderSubPixel(int y, World w, ConcurrentDictionary<SubPixel, Color> samples)
        {
            for (var x = 0; x <= HSize; x++)
            {
                var sp = new SubPixel(x, y, 1, 0, 0);
                var color = ColorAtSubPixel(w, sp);
                samples.TryAdd(sp, color);
            }
        }

        private Func<SubPixel, Color> SubPixelRenderFunc(World w)
        {
            return pixel => ColorAtSubPixel(w, in pixel);
        }

        private Color ColorAtSubPixel(World world, in SubPixel sp)
        {
            var dx = (float)sp.Dx / sp.Divisions;
            var dy = (float)sp.Dy/ sp.Divisions;
            var xOffset = (sp.X + dx) * PixelSize;
            var yOffset = (sp.Y + dy) * PixelSize;

            //var xOffset = (px + 0.5f) * PixelSize;
            //var yOffset = (py + 0.5f) * PixelSize;
            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var inv = _transformInverse;
            var pixel = inv * new Point(worldX, worldY, -1f);
            var origin = inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            var r = new Ray(origin, direction);
            return Shading.ColorAt(world, r, 4);
        }

        private void RenderRow(World world, int y, Canvas canvas)
        {
            for (var x = 0; x < HSize; x++)
            {
                RenderPixel(world, x, y, canvas);
            }
        }

        private void RenderPixel(World world, int x, int y, Canvas canvas)
        {
            var ray = RayForPixel(x, y);
            var color = Shading.ColorAt(world, ray, 4);
            canvas.WritePixel(color, x, y);
        }

        public readonly struct SubPixel : IEquatable<SubPixel>
        {
            public int X { get; }
            public int Y { get; }
            public ushort Divisions { get; }
            public ushort Dx { get; }
            public ushort Dy { get; }

            public SubPixel(int x, int y, ushort divisions, ushort dx, ushort dy)
            {
                X = x;
                Y = y;
                Divisions = divisions;
                Dx = dx;
                Dy = dy;
            }

            public bool Equals(SubPixel other) => X == other.X && Y == other.Y && Divisions == other.Divisions && Dx == other.Dx && Dy == other.Dy;

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
                var tl = Create(x, y, div, (ushort)(dx - 1), (ushort)(dy - 1));
                var tr = Create(x, y, div, (ushort)(dx + 1), (ushort)(dy - 1));
                var bl = Create(x, y, div, (ushort)(dx - 1), (ushort)(dy + 1));
                var br = Create(x, y, div, (ushort)(dx + 1), (ushort)(dy + 1));
                return (tl, tr, bl, br);
            }

            public static SubPixel Create(int x, int y, ushort divisions, ushort dx, ushort dy)
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

                while (dx % 2 == 0 && dy % 2 == 0 && divisions % 2 == 0)
                {
                    dx = (ushort)(dx >>1);
                    dy = (ushort)(dy >> 1);
                    divisions = (ushort)(divisions >> 1);
                }
                return new SubPixel(x,y,divisions,dx,dy);
            }

            private static SubPixel ToDivResolution(in SubPixel sp, ushort resolution)
            {
                if (sp.Divisions == resolution) return sp;
                var fac =  resolution / sp.Divisions;
                return new SubPixel(sp.X,sp.Y, resolution,(ushort)(sp.Dx* fac),(ushort)(sp.Dy* fac));
            }

            public static SubPixel Center(in SubPixel tl, in SubPixel br)
            {
                var div = (ushort) Math.Max(tl.Divisions<<1, br.Divisions<<1);
                var utl = ToDivResolution(in tl, div);
                var ubr = ToDivResolution(in br, div);
                var xl = utl.X * div + utl.Dx;
                var xr = ubr.X * div + ubr.Dx;
                var yt = utl.Y * div + utl.Dy;
                var yb = ubr.Y * div + ubr.Dy;
                var dx = Math.Min(xl,xr) + Math.Abs(xr - xl) / 2;
                var dy = Math.Min(yt,yb) + Math.Abs(yb - yt) / 2;
                var x = dx / div;
                dx -= x*div;
                var y = dy / div;
                dy -= y*div;
                return Create(x, y, div, (ushort)dx, (ushort)dy);
            }
        }
    }
}