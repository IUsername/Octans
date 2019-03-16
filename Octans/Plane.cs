using System;
using System.Collections.Generic;

namespace Octans
{
    public class Plane : ShapeBase
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

        public override Vector LocalNormalAt(in Point localPoint) => new Vector(0, 1, 0);


        // TODO: Epsilon?
        public override Bounds LocalBounds() => new Bounds(new Point(float.NegativeInfinity, 0, float.NegativeInfinity),
                                                           new Point(float.PositiveInfinity, 0, float.PositiveInfinity));
    }
}