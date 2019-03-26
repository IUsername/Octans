using System;

namespace Octans.Geometry
{
    public class Cube : GeometryBase
    {
        private const float Epsilon = 0.0001f;

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var (xtMin, xtMax) = CheckAxis(localRay.Origin.X, localRay.Direction.X);
            var (ytMin, ytMax) = CheckAxis(localRay.Origin.Y, localRay.Direction.Y);
            var (ztMin, ztMax) = CheckAxis(localRay.Origin.Z, localRay.Direction.Z);

            var tMin = MathF.Max(MathF.Max(xtMin, ytMin), ztMin);
            var tMax = MathF.Min(MathF.Min(xtMax, ytMax), ztMax);

            return tMin > tMax
                ? Intersections.Empty()
                : Intersections.Create(new Intersection(tMin, this), new Intersection(tMax, this));
        }

        private static (float min, float max) CheckAxis(float origin, float direction)
        {
            var tMinNum = -1f - origin;
            var tMaxNum = 1 - origin;
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

        public override Vector LocalNormalAt(in Point localPoint, in Intersection intersection)
        {
            var xAbs = MathF.Abs(localPoint.X);
            var yAbs = MathF.Abs(localPoint.Y);
            var zAbs = MathF.Abs(localPoint.Z);

            var maxC = MathF.Max(MathF.Max(xAbs, yAbs), zAbs);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            return maxC == xAbs
                ? new Vector(localPoint.X, 0, 0)
                : maxC == yAbs
                    ? new Vector(0, localPoint.Y, 0)
                    : new Vector(0, 0, localPoint.Z);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Bounds LocalBounds() => Bounds.Unit;
    }
}