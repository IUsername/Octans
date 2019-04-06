using System;

namespace Octans.Geometry
{
    public class ConstructiveSolid : GeometryBase
    {
        private readonly Bounds _bounds;

        public ConstructiveSolid(ConstructiveOp op, IGeometry left, IGeometry right)
        {
            Op = op;
            left.Parent = this;
            right.Parent = this;
            Left = left;
            Right = right;
            _bounds = Left.ParentSpaceBounds() + Right.ParentSpaceBounds();
        }

        public ConstructiveOp Op { get; }
        public IGeometry Left { get; }
        public IGeometry Right { get; }

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            if (!LocalBounds().DoesIntersect(localRay))
            {
                return Intersections.Empty();
            }

            var builder = Intersections.Builder();
            var left = Left.Intersects(in localRay);
            var right = Right.Intersects(in localRay);
            builder.AddRange(in left);
            builder.AddRange(in right);
            var xs = builder.ToIntersections();
            var result = FilterIntersections(in xs);
            left.Return();
            right.Return();
            xs.Return();
            return result;
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            throw new NotImplementedException();

        public override Bounds LocalBounds() => _bounds;

        public static bool IntersectionAllowed(ConstructiveOp op, bool lHit, bool inL, bool inR)
        {
            switch (op)
            {
                case ConstructiveOp.Union:
                    return (lHit & !inR) | (!lHit & !inL);
                case ConstructiveOp.Intersection:
                    return (lHit & inR) | (!lHit & inL);
                case ConstructiveOp.Difference:
                    return (lHit & !inR) | (!lHit & inL);
                default:
                    return false;
            }
        }

        public IIntersections FilterIntersections(in IIntersections intersections)
        {
            if (intersections.Count == 0)
            {
                return Intersections.Empty();
            }

            var inL = false;
            var inR = false;

            var result = Intersections.Builder();
            var sorted = intersections.ToSorted();
            foreach (var i in sorted)
            {
                var lHit = Left.Includes(i.Geometry);
                if (IntersectionAllowed(Op, lHit, inL, inR))
                {
                    result.Add(i);
                }

                if (lHit)
                {
                    inL = !inL;
                }
                else
                {
                    inR = !inR;
                }
            }

            return result.ToIntersections();
        }

        public override void Divide(int threshold)
        {
            Left.Divide(threshold);
            Right.Divide(threshold);
        }
    }
}