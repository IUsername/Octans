using Octans.Geometry;

namespace Octans.Test.Geometry
{
    internal class TestGeometry : GeometryBase
    {
        public Ray SavedRay { get; private set; }

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            SavedRay = localRay;
            return Intersections.Empty();
        }

        public override Vector LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            new Vector(localPoint.X, localPoint.Y, localPoint.Z);

        public override Bounds LocalBounds() => Bounds.Unit;
    }
}