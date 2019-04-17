using static System.MathF;

namespace Octans.Geometry
{
    public class Cube : GeometryBase
    {
        //private const float Epsilon = 0.0001f;

        //public override IIntersections LocalIntersects(in Ray localRay)
        //{
        //    var (xtMin, xtMax) = CheckAxis(localRay.Origin.X, localRay.Direction.X);
        //    var (ytMin, ytMax) = CheckAxis(localRay.Origin.Y, localRay.Direction.Y);
        //    var (ztMin, ztMax) = CheckAxis(localRay.Origin.Z, localRay.Direction.Z);

        //    var tMin = MathF.Max(MathF.Max(xtMin, ytMin), ztMin);
        //    var tMax = MathF.Min(MathF.Min(xtMax, ytMax), ztMax);

        //    return tMin > tMax
        //        ? Intersections.Empty()
        //        : Intersections.Create(new Intersection(tMin, this), new Intersection(tMax, this));
        //}

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var t1 = (-1f - localRay.Origin.X) * localRay.InverseDirection.X;
            var t2 = (1f - localRay.Origin.X) * localRay.InverseDirection.X;
            var t3 = (-1f - localRay.Origin.Y) * localRay.InverseDirection.Y;
            var t4 = (1f - localRay.Origin.Y) * localRay.InverseDirection.Y;
            var t5 = (-1f - localRay.Origin.Z) * localRay.InverseDirection.Z;
            var t6 = (1f - localRay.Origin.Z) * localRay.InverseDirection.Z;

            var tMax = Min(Min(Max(t1, t2), Max(t3, t4)), Max(t5, t6));

            //var t = 0f;
            if (tMax < 0f)
            {
                return Intersections.Empty();
            }

            var tMin = Max(Max(Min(t1, t2), Min(t3, t4)), Min(t5, t6));
            if (tMin > tMax)
            {
                return Intersections.Empty();
            }

            if (float.IsNaN(tMax))
            {
                return Intersections.Empty();
            }

            //t = tMin;
            return Intersections.Create(new Intersection(tMin, this), new Intersection(tMax, this));
        }

        //private static (float min, float max) CheckAxis(float origin, float direction)
        //{
        //    var tMinNum = -1f - origin;
        //    var tMaxNum = 1 - origin;
        //    float tMin, tMax;
        //    if (Abs(direction) >= Epsilon)
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

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection)
        {
            var abs = Point.Abs(in localPoint);
            var maxC = Point.Max(in abs);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return maxC == abs.X
                ? new Normal(localPoint.X, 0, 0)
                : maxC == abs.Y
                    ? new Normal(0, localPoint.Y, 0)
                    : new Normal(0, 0, localPoint.Z);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Bounds LocalBounds() => Bounds.Unit;
    }
}