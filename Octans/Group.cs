using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Octans
{
    public class Group : ShapeBase
    {
        private readonly List<IShape> _children;

        public Group()
        {
            _children = new List<IShape>();
        }

        public IReadOnlyCollection<IShape> Children => _children;

        public override IReadOnlyList<Intersection> LocalIntersects(in Ray localRay)
        {
            if (_children.Count == 0)
            {
                return Intersections.Empty;
            }

            var bounds = LocalBounds();
            if (!bounds.DoesIntersect(localRay))
            {
                return Intersections.Empty;
            }

            var intersections = new List<Intersection>();
            foreach (var child in _children)
            {
                intersections.AddRange(child.Intersects(in localRay));
            }

            return new Intersections(intersections);
        }

        public override Vector LocalNormalAt(in Point localPoint) => throw new NotImplementedException();

        public override Bounds LocalBounds()
        {
            return Children.Aggregate(Bounds.Empty, (current, child) => current + ToLocalBounds(child));
        }

        [Pure]
        private static Bounds ToLocalBounds(IShape child)
        {
            var corners = Bounds.ToCornerPoints(child.LocalBounds());
            var localPoints = corners.Select(point => child.Transform * point).ToArray();
            return Bounds.FromPoints(localPoints);
        }

        public void AddChild(IShape shape)
        {
            _children.Add(shape);
            shape.Parent = this;
        }
    }
}