using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Octans
{
    public class Solid : ShapeBase
    {
        public Solid(SolidOp op, IShape left, IShape right)
        {
            Op = op;
            left.Parent = this;
            right.Parent = this;
            Left = left;
            Right = right;
            _bounds = new Lazy<Bounds>(BoundsFactory);
        }

        public SolidOp Op { get; }
        public IShape Left { get; }
        public IShape Right { get; }

        private readonly Lazy<Bounds> _bounds;

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var builder = Intersections.Builder();
            builder.AddRange(Left.Intersects(in localRay));
            builder.AddRange(Right.Intersects(in localRay));
            return FilterIntersections(builder.ToIntersections());
        }

        public override Vector LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            throw new NotImplementedException();

        public override Bounds LocalBounds()
        {
            return _bounds.Value;
        }

        private Bounds BoundsFactory()
        {
            return ToLocalBounds(Left) + ToLocalBounds(Right);
        }

        [Pure]
        private static Bounds ToLocalBounds(IShape child)
        {
            var corners = Bounds.ToCornerPoints(child.LocalBounds());
            var localPoints = corners.Select(point => child.Transform * point).ToArray();
            return Bounds.FromPoints(localPoints);
        }

        public static bool IntersectionAllowed(SolidOp op, bool lHit, bool inL, bool inR)
        {
            switch (op)
            {
                case SolidOp.Union:
                    return (lHit & !inR) | (!lHit & !inL);
                case SolidOp.Intersection:
                    return (lHit & inR) | (!lHit & inL);
                case SolidOp.Difference:
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
                var lHit = Left.Includes(i.Shape);
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
    }

    internal static class SolidExtensions
    {
        public static bool Includes(this IShape a, IShape b)
        {
            switch (a)
            {
                case Group g when g.Children.Any(c => ReferenceEquals(a,c) || c.Includes(b)):
                    return true;
                case Solid s:
                    return s.Left.Includes(b) || s.Right.Includes(b);
                default:
                    return ReferenceEquals(a, b);
            }
        }
    }

    public enum SolidOp
    {
        Union,
        Intersection,
        Difference
    }
}