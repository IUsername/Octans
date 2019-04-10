using System;
using System.Collections.Generic;

namespace Octans.Geometry
{
    public class Group : GeometryBase
    {
        private Bounds _bounds;
        private readonly List<IGeometry> _children;

        public Group()
        {
            _children = new List<IGeometry>();
            _bounds = Bounds.Empty;
        }

        public Group(params IGeometry[] geometries)
        {
            _children = new List<IGeometry>(geometries);
            _bounds = BoundsFactory();
        }

        public IReadOnlyList<IGeometry> Children => _children;

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var bounds = LocalBounds();
            if (!bounds.DoesIntersect(localRay))
            {
                return Intersections.Empty();
            }

            var intersections = Intersections.Builder();
            for (var index = 0; index < _children.Count; index++)
            {
                var child = _children[index];
                var xs = child.Intersects(in localRay);
                intersections.AddRange(in xs);
                xs.Return();
            }

            return intersections.ToIntersections();
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            throw new NotImplementedException();

        public override Bounds LocalBounds() => _bounds;

        private Bounds BoundsFactory()
        {
            var result = Bounds.Empty;
            for (var index = 0; index < Children.Count; index++)
            {
                var child = Children[index];
                result += child.ParentSpaceBounds();
            }

            return result;
        }

        public void AddChild(IGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            if (ReferenceEquals(geometry, this))
            {
                throw new InvalidOperationException("Cannot add self to group");
            }

            _children.Add(geometry);
            geometry.Parent = this;
            _bounds = BoundsFactory();
        }

        public (IGeometry[] left, IGeometry[] right) PartitionChildren()
        {
            var (l, r) = LocalBounds().Split();
            var left = new List<IGeometry>();
            var right = new List<IGeometry>();
            var remaining = new List<IGeometry>();
            for (var index = 0; index < _children.Count; index++)
            {
                var child = _children[index];
                var cb = child.ParentSpaceBounds();
                if (l.ContainsBounds(cb))
                {
                    left.Add(child);
                }
                else if (r.ContainsBounds(cb))
                {
                    right.Add(child);
                }
                else
                {
                    remaining.Add(child);
                }
            }

            _children.Clear();
            _children.AddRange(remaining);
            return (left.ToArray(), right.ToArray());
        }

        public Group AddSubgroup(IGeometry[] geometries)
        {
            var g = new Group(geometries);
            AddChild(g);
            return g;
        }

        public override void Divide(int threshold)
        {
            if (threshold <= Children.Count)
            {
                var (l, r) = PartitionChildren();
                if (l.Length > 0)
                {
                    AddSubgroup(l);
                }

                if (r.Length > 0)
                {
                    AddSubgroup(r);
                }
            }

            // Always divide children.
            foreach (var child in Children)
            {
                child.Divide(threshold);
            }
        }
    }
}