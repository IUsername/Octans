using System;

namespace Octans.Camera
{
    public class ApertureCamera : ICamera
    {
        private readonly Point _camera;
        private readonly float _focalDist;
        private readonly float _halfHeight;
        private readonly float _halfWidth;
        private readonly Matrix _inv;
        private readonly Sequence<Vector> _jitter;
        private readonly float _pixelSize;

        public ApertureCamera(float fieldOfView,
                              int hSize,
                              int vSize,
                              float apertureRadius,
                              Point from,
                              Point to,
                              float focalDistance = -1)
        {
            ApertureRadius = apertureRadius;
            var transform = Transforms.View(from, to, new Vector(0, 1, 0));
            _inv = transform.Inverse();
            _camera = _inv * Point.Zero;
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
            _pixelSize = halfWidth * 2f / hSize;
            _focalDist = focalDistance < 0 ? (from - to).Magnitude() : focalDistance;

            _jitter = BuildJitter(in _inv, apertureRadius);
        }

        public float ApertureRadius { get; }

        public Color Render(IScene scene, in SubPixel sp)
        {
            var aimed = FocalPlanePoint(in sp);
            var total = Colors.Black;
            var prior = total;
            // TODO: Parameterize
            int minRays = 4;
            int incrementRays = 4;
            int maxRays = 32;
            var delta = 0.005f;
            int count = 1;
            for (; count <= maxRays; count++)
            {
                var start = _camera + _jitter.Next();
                var dir = (aimed - start).Normalize();
                var r = new Ray(start, dir);
                var c = scene.World.ColorFor(in r);
                if (count == 1)
                {
                    prior = c;
                }

                total += c;
                
                if (count < minRays)
                {
                    continue;
                }

                if (count != minRays && (count - minRays) % incrementRays != 0)
                {
                    continue;
                }

                var current = total / count;
                if (Color.IsWithinPerceptiveDelta(in current, in prior, delta))
                {
                    break;
                }

                prior = current;
            }

            return total / count; 
        }

        //private static Sequence<Vector> BuildJitter(in Matrix transformInv, float apertureRadius)
        //{
        //    var uRadius = transformInv * new Vector(1, 0, 0) * apertureRadius;
        //    var vRadius = transformInv * new Vector(0, 1, 0) * apertureRadius;
        //    var rSqr = vRadius.MagSqr();

        //    // TODO: +X,+Y to theta,phi to avoid radius checks
        //    var unitSeq = Sequence.LargeRandomUnit();
        //    var count = 127;
        //    var jitter = new Vector[count];
        //    for (var i = 0; i < count;)
        //    {
        //        var u = uRadius * unitSeq.Next();
        //        var v = vRadius * unitSeq.Next();
        //        var offset = u + v;
        //        if (offset.MagSqr() > rSqr)
        //        {
        //            continue;
        //        }

        //        jitter[i++] = offset;
        //    }

        //    return new Sequence<Vector>(jitter);
        //}

        private static Sequence<Vector> BuildJitter(in Matrix transformInv, float apertureRadius)
        {
            var uRadius = transformInv * new Vector(1, 0, 0) * apertureRadius;
            var vRadius = transformInv * new Vector(0, 1, 0) * apertureRadius;
            var count = 127;
            var jitter = new Vector[count];
            for (var i = 0; i < count;)
            {
                var (u1, u2) = QuasiRandom.Next(i);
                var r = MathF.Sqrt(1f - u1 * u1);
                var phi = 2 * MathF.PI * u2;
                var u = uRadius * MathF.Cos(phi) * r;
                var v = vRadius * MathF.Sin(phi) * r;
                var offset = u + v;
                jitter[i++] = offset;
            }
            return new Sequence<Vector>(jitter);
        }

        private Point FocalPlanePoint(in SubPixel sp)
        {
            var dx = (float) sp.Dx / sp.Divisions;
            var dy = (float) sp.Dy / sp.Divisions;
            var xOffset = (sp.X + dx) * _pixelSize;
            var yOffset = (sp.Y + dy) * _pixelSize;

            var worldX = _halfWidth - xOffset;
            var worldY = _halfHeight - yOffset;

            var pixel = _inv * new Point(worldX, worldY, -1f);
            var origin = _inv * Point.Zero;
            var direction = (pixel - origin).Normalize();
            var r = new Ray(origin, direction);
            return r.Position(_focalDist);
        }
    }
}