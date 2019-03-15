using System;
using System.Collections.Generic;
using System.Linq;

namespace Octans
{
    /// <summary>
    ///     Axis-aligned bounding boxes (AABB).
    /// </summary>
    public readonly struct Bounds
    {
        private const float Epsilon = 0.0001f;

        public Point Min { get; }
        public Point Max { get; }

        public bool IsEmpty { get; }

        public Bounds(Point min, Point max) : this(min, max, false)
        {
        }

        private Bounds(Point min, Point max, bool isEmpty)
        {
            Min = min;
            Max = max;
            IsEmpty = isEmpty;
        }

        public static Point[] ToCornerPoints(in Bounds b) =>
            new[]
            {
                b.Min,
                b.Max,
                new Point(b.Min.X, b.Min.Y, b.Max.Z),
                new Point(b.Min.X, b.Max.Y, b.Min.Z),
                new Point(b.Max.X, b.Min.Y, b.Min.Z),
                new Point(b.Min.X, b.Max.Y, b.Max.Z),
                new Point(b.Max.X, b.Min.Y, b.Max.Z),
                new Point(b.Max.X, b.Max.Y, b.Min.Z)
            };

        public static Bounds FromPoints(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            var minX = enumerable.Min(p => p.X);
            var minY = enumerable.Min(p => p.Y);
            var minZ = enumerable.Min(p => p.Z);

            var maxX = enumerable.Max(p => p.X);
            var maxY = enumerable.Max(p => p.Y);
            var maxZ = enumerable.Max(p => p.Z);

            if (float.IsNaN(minX))
            {
                // Odd case where an transform produces NaN for points. Give up and return global bounds.
                return new Bounds(new Point(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
                                  new Point(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
            }

            return new Bounds(new Point(minX, minY, minZ), new Point(maxX, maxY, maxZ));
        }


        private static (float min, float max) CheckAxis(float origin, float direction, float bMin, float bMax)
        {
            var tMinNum = bMin - origin;
            var tMaxNum = bMax - origin;
            float tMin, tMax;
            if (MathF.Abs(direction) >= Epsilon)
            {
                tMin = tMinNum / direction;
                tMax = tMaxNum / direction;
            }
            else
            {
                tMin = float.IsNegative(tMinNum) ? float.NegativeInfinity : float.PositiveInfinity;
                tMax = float.IsNegative(tMaxNum) ? float.NegativeInfinity : float.PositiveInfinity;
            }

            return tMin > tMax ? (tmin: tMax, tmax: tMin) : (tmin: tMin, tmax: tMax);
        }

        public bool DoesIntersect(in Ray ray)
        {
            var (xtMin, xtMax) = CheckAxis(ray.Origin.X, ray.Direction.X, Min.X, Max.X);
            var (ytMin, ytMax) = CheckAxis(ray.Origin.Y, ray.Direction.Y, Min.Y, Max.Y);
            var (ztMin, ztMax) = CheckAxis(ray.Origin.Z, ray.Direction.Z, Min.Z, Max.Z);

            var tMin = MathF.Max(MathF.Max(xtMin, ytMin), ztMin);
            var tMax = MathF.Min(MathF.Min(xtMax, ytMax), ztMax);

            return tMin <= tMax;
        }

        public static Bounds Empty => new Bounds(Point.Zero, Point.Zero, true);

        public static Bounds Add(in Bounds a, in Bounds b)
        {
            if (a.IsEmpty)
            {
                return b;
            }

            if (b.IsEmpty)
            {
                return a;
            }

            var minX = MathF.Min(a.Min.X, b.Min.X);
            var minY = MathF.Min(a.Min.Y, b.Min.Y);
            var minZ = MathF.Min(a.Min.Z, b.Min.Z);
            var min = new Point(minX, minY, minZ);

            var maxX = MathF.Max(a.Max.X, b.Max.X);
            var maxY = MathF.Max(a.Max.Y, b.Max.Y);
            var maxZ = MathF.Max(a.Max.Z, b.Max.Z);
            var max = new Point(maxX, maxY, maxZ);

            return new Bounds(min, max);
        }

        public static Bounds operator +(in Bounds left, in Bounds right) => Add(in left, in right);
    }
}