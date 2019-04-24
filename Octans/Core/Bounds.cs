using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using static System.MathF;
using static System.Single;

namespace Octans
{
    /// <summary>
    ///     Axis-aligned bounding boxes (AABB).
    /// </summary>
    public readonly struct Bounds
    {
        //private const float Epsilon = 0.0001f;

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

        public static Bounds FromPoints(params Point[] points) => FromPoints(points.AsEnumerable());

        public static Bounds FromPoints(IEnumerable<Point> points)
        {
            var enumerable = points as Point[] ?? points.ToArray();
            var minX = enumerable.Min(p => p.X);
            var minY = enumerable.Min(p => p.Y);
            var minZ = enumerable.Min(p => p.Z);

            var maxX = enumerable.Max(p => p.X);
            var maxY = enumerable.Max(p => p.Y);
            var maxZ = enumerable.Max(p => p.Z);

            if (IsNaN(minX))
            {
                // Odd case where an transform produces NaN for points. Give up and return global bounds.
                return Infinite;
            }

            return new Bounds(new Point(minX, minY, minZ), new Point(maxX, maxY, maxZ));
        }


        //private static (float min, float max) CheckAxis(float origin, float direction, float bMin, float bMax)
        //{
        //    var tMinNum = bMin - origin;
        //    var tMaxNum = bMax - origin;
        //    float tMin, tMax;
        //    if (MathF.Abs(direction) >= Epsilon)
        //    {
        //        tMin = tMinNum / direction;
        //        tMax = tMaxNum / direction;
        //    }
        //    else
        //    {
        //        tMin = float.IsNegative(tMinNum) ? float.NegativeInfinity : float.PositiveInfinity;
        //        tMax = float.IsNegative(tMaxNum) ? float.NegativeInfinity : float.PositiveInfinity;
        //    }

        //    return tMin > tMax ? (tmin: tMax, tmax: tMin) : (tmin: tMin, tmax: tMax);
        //}

        public bool DoesIntersect(in Ray ray)
        {
            if (IsEmpty)
            {
                return false;
            }

            var t1 = (Min.X - ray.Origin.X) * ray.InverseDirection.X;
            var t2 = (Max.X - ray.Origin.X) * ray.InverseDirection.X;
            var t3 = (Min.Y - ray.Origin.Y) * ray.InverseDirection.Y;
            var t4 = (Max.Y - ray.Origin.Y) * ray.InverseDirection.Y;
            var t5 = (Min.Z - ray.Origin.Z) * ray.InverseDirection.Z;
            var t6 = (Max.Z - ray.Origin.Z) * ray.InverseDirection.Z;

            var tMax = Min(Min(Max(t1, t2), Max(t3, t4)), Max(t5, t6));

            //var t = 0f;
            if (tMax < 0f)
            {
                //t = tMax;
                return false;
            }

            var tMin = Max(Max(Min(t1, t2), Min(t3, t4)), Min(t5, t6));
            if (tMin > tMax)
            {
                //t = tMax;
                return false;
            }

            if (IsNaN(tMax))
            {
                //t = float.PositiveInfinity;
                return false;
            }

            //t = tMin;
            return true;
        }

        //public bool DoesIntersect(in Ray ray)
        //{
        //    var (xtMin, xtMax) = CheckAxis(ray.Origin.X, ray.Direction.X, Min.X, Max.X);
        //    var (ytMin, ytMax) = CheckAxis(ray.Origin.Y, ray.Direction.Y, Min.Y, Max.Y);
        //    var (ztMin, ztMax) = CheckAxis(ray.Origin.Z, ray.Direction.Z, Min.Z, Max.Z);

        //    var tMin = MathF.Max(MathF.Max(xtMin, ytMin), ztMin);
        //    var tMax = MathF.Min(MathF.Min(xtMax, ytMax), ztMax);

        //    return tMin <= tMax;
        //}

        public static Bounds Empty => new Bounds(Point.Zero, Point.Zero, true);

        public static Bounds Infinite =>
            new Bounds(new Point(NegativeInfinity, NegativeInfinity, NegativeInfinity),
                       new Point(PositiveInfinity, PositiveInfinity, PositiveInfinity));

        public static Bounds Unit => new Bounds(new Point(-1, -1, -1), new Point(1, 1, 1));

        public Point Centroid => Min * 0.5f + (Vector) Max * 0.5f;

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

            var minX = Min(a.Min.X, b.Min.X);
            var minY = Min(a.Min.Y, b.Min.Y);
            var minZ = Min(a.Min.Z, b.Min.Z);
            var min = new Point(minX, minY, minZ);

            var maxX = Max(a.Max.X, b.Max.X);
            var maxY = Max(a.Max.Y, b.Max.Y);
            var maxZ = Max(a.Max.Z, b.Max.Z);
            var max = new Point(maxX, maxY, maxZ);

            return new Bounds(min, max);
        }

        public static Bounds Add(in Bounds a, in Point b)
        {
            if (a.IsEmpty)
            {
                return new Bounds(b, b);
            }

            var minX = Min(a.Min.X, b.X);
            var minY = Min(a.Min.Y, b.Y);
            var minZ = Min(a.Min.Z, b.Z);
            var min = new Point(minX, minY, minZ);

            var maxX = Max(a.Max.X, b.X);
            var maxY = Max(a.Max.Y, b.Y);
            var maxZ = Max(a.Max.Z, b.Z);
            var max = new Point(maxX, maxY, maxZ);

            return new Bounds(min, max);
        }

        public static Bounds operator +(in Bounds left, in Bounds right) => Add(in left, in right);
        public static Bounds operator +(in Bounds left, in Point right) => Add(in left, in right);

        [Pure]
        public bool ContainsPoint(Point point) =>
            point.X >= Min.X && point.X <= Max.X
                             && point.Y >= Min.Y && point.Y <= Max.Y
                             && point.Z >= Min.Z && point.Z <= Max.Z;

        [Pure]
        public bool ContainsBounds(Bounds bounds) => ContainsPoint(bounds.Min) && ContainsPoint(bounds.Max);

        [Pure]
        public (Bounds left, Bounds right) Split()
        {
            var wX = Max.X - Min.X;
            var wY = Max.Y - Min.Y;
            var wZ = Max.Z - Min.Z;
            var maxW = Max(wX, Max(wY, wZ));
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (maxW == wX)
            {
                var midX = Min.X + wX / 2f;
                return (new Bounds(Min, new Point(midX, Max.Y, Max.Z)), new Bounds(new Point(midX, Min.Y, Min.Z), Max));
            }

            if (maxW == wY)
            {
                var midY = Min.Y + wY / 2f;
                return (new Bounds(Min, new Point(Max.X, midY, Max.Z)), new Bounds(new Point(Min.X, midY, Min.Z), Max));
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var midZ = Min.Z + wZ / 2f;
            return (new Bounds(Min, new Point(Max.X, Max.Y, midZ)), new Bounds(new Point(Min.X, Min.Y, midZ), Max));
        }

        [Pure]
        public int MaximumExtent()
        {
            var d = Max - Min;
            return d.X > d.Y && d.X > d.Z ? 0 : d.Y > d.Z ? 1 : 2;
        }

        [Pure]
        public Vector Offset(in Point p)
        {
            var o = p - Min;
            var x = o.X;
            var y = o.Y;
            var z = o.Z;
            if (Max.X > Min.X)
            {
                x /= Max.X - Min.X;
            }

            if (Max.Y > Min.Y)
            {
                y /= Max.Y - Min.Y;
            }

            if (Max.Z > Min.Z)
            {
                z /= Max.Z - Min.Z;
            }

            return new Vector(x, y, z);
        }

        [Pure]
        public float SurfaceArea()
        {
            var d = Max - Min;
            return 2f * (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
        }

        public Point this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Min;
                    case 1: return Max;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public bool IntersectP(Ray ray, int[] dirIsNeg)
        {
            var tMin = (this[dirIsNeg[0]].X - ray.Origin.X) * ray.InverseDirection.X;
            var tMax = (this[1 - dirIsNeg[0]].X - ray.Origin.X) * ray.InverseDirection.X;
            var tyMin = (this[dirIsNeg[1]].Y - ray.Origin.Y) * ray.InverseDirection.Y;
            var tyMax = (this[1 - dirIsNeg[1]].Y - ray.Origin.Y) * ray.InverseDirection.Y;

            tMax *= 1f + 2f * MathF.Gamma(3);
            tyMax *= 1f + 2f * MathF.Gamma(3);
            if (tMin > tyMax || tyMin > tMax)
            {
                return false;
            }

            if (tyMin > tMin)
            {
                tMin = tyMin;
            }

            if (tyMax < tMax)
            {
                tMax = tyMax;
            }

            var tzMin = (this[dirIsNeg[2]].Z - ray.Origin.Z) * ray.InverseDirection.Z;
            var tzMax = (this[1 - dirIsNeg[2]].Z - ray.Origin.Z) * ray.InverseDirection.Z;

            tzMax *= 1f + 2f * MathF.Gamma(3);
            if (tMin > tzMax || tzMin > tMax)
            {
                return false;
            }

            if (tzMin > tMin)
            {
                tMin = tzMin;
            }

            if (tzMax < tMax)
            {
                tMax = tzMax;
            }

            return tMin < ray.TMax && tMax > 0;
        }
    }
}