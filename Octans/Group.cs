using System;
using System.Collections.Generic;
using System.Linq;

namespace Octans
{
    public class Group : ShapeBase
    {
        private readonly Lazy<Bounds> _bounds;
        private readonly List<IShape> _children;

        public Group()
        {
            _children = new List<IShape>();
            _bounds = new Lazy<Bounds>(BoundsFactory);
        }

        public Group(params IShape[] shapes)
        {
            _children = new List<IShape>(shapes);
            _bounds = new Lazy<Bounds>(BoundsFactory);
        }

        public IReadOnlyList<IShape> Children => _children;

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var bounds = LocalBounds();
            if (!bounds.DoesIntersect(localRay))
            {
                return Intersections.Empty();
            }

            var intersections = Intersections.Builder();
            foreach (var child in _children)
            {
                intersections.AddRange(child.Intersects(in localRay));
            }

            return intersections.ToIntersections();
        }

        public override Vector LocalNormalAt(in Point localPoint, in Intersection intersection) =>
            throw new NotImplementedException();

        public override Bounds LocalBounds() => _bounds.Value;

        private Bounds BoundsFactory()
        {
            return Children.Aggregate(Bounds.Empty, (current, child) => current + child.ParentSpaceBounds());
        }

        public void AddChild(IShape shape)
        {
            _children.Add(shape);
            shape.Parent = this;
        }

        public (IShape[] left, IShape[] right) PartitionChildren()
        {
            var (l, r) = LocalBounds().Split();
            var left = new List<IShape>();
            var right = new List<IShape>();
            var remaining = new List<IShape>();
            foreach (var child in _children)
            {
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

        public Group AddSubgroup(IShape[] shapes)
        {
            var g = new Group(shapes);
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