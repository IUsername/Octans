using System;

namespace Octans.Geometry
{
    public class Plane : GeometryBase
    {
        private const float Epsilon = 0.0001f;

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            // XZ plane
            if (MathF.Abs(localRay.Direction.Y) < Epsilon)
            {
                return Intersections.Empty();
            }

            var t = -localRay.Origin.Y / localRay.Direction.Y;
            return Intersections.Create(new Intersection(t, this));
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection) => new Normal(0, 1, 0);


        // TODO: Epsilon?
        public override Bounds LocalBounds() => new Bounds(new Point(float.NegativeInfinity, 0, float.NegativeInfinity),
                                                           new Point(float.PositiveInfinity, 0,
                                                                     float.PositiveInfinity));
    }
}