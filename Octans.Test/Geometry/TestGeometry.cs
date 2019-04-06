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

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            new Normal(localPoint.X, localPoint.Y, localPoint.Z);

        public override Bounds LocalBounds() => Bounds.Unit;
    }
}