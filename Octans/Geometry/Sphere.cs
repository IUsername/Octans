using System;

namespace Octans.Geometry
{
    public class Sphere : GeometryBase
    {
        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var sphereToRay = localRay.Origin - Point.Zero;
            var a = localRay.Direction % localRay.Direction;
            var b = 2f * localRay.Direction % sphereToRay;
            var c = sphereToRay % sphereToRay - 1f;
            var discriminant = b * b - 4 * a * c;
            if (discriminant < 0f)
            {
                return Intersections.Empty();
            }

            discriminant = System.MathF.Sqrt(discriminant);
            var denominator = 2f * a;
            var t1 = (-b - discriminant) / denominator;
            var t2 = (-b + discriminant) / denominator;
            return Intersections.Create(
                new Intersection(t1, this),
                new Intersection(t2, this));
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            (Normal)(localPoint - Point.Zero);

        public override Bounds LocalBounds() => Bounds.Unit;
    }
}