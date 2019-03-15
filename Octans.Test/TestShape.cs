using System.Collections.Generic;

namespace Octans.Test
{
    internal class TestShape : ShapeBase
    {
        public Ray SavedRay { get; private set; }

        public override IReadOnlyList<Intersection> LocalIntersects(in Ray localRay)
        {
            SavedRay = localRay;
            return Intersections.Empty;
        }

        public override Vector LocalNormalAt(in Point localPoint) =>
            new Vector(localPoint.X, localPoint.Y, localPoint.Z);

        public override Bounds LocalBounds() =>
            new Bounds(new Point(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
                       new Point(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
    }
}