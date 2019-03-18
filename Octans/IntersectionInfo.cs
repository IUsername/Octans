using System.Collections.Generic;

namespace Octans
{
    public readonly struct IntersectionInfo
    {
        public IntersectionInfo(Intersection intersection, Ray ray) : this(intersection, ray, new[] {intersection})
        {
        }

        public IntersectionInfo(
            Intersection intersection,
            Ray ray,
            IEnumerable<Intersection> intersections)
        {
            T = intersection.T;
            Shape = intersection.Shape;
            Point = ray.Position(T);
            Eye = -ray.Direction;
            Normal = Shape.NormalAt(in Point, in intersection);
            if (Normal % Eye < 0f)
            {
                IsInside = true;
                Normal = -Normal;
            }
            else
            {
                IsInside = false;
            }

            var offset = Normal * Epsilon;
            OverPoint = Point + offset;
            UnderPoint = Point - offset;
            Reflect = Vector.Reflect(in ray.Direction, in Normal);

            N1 = 1.0f;
            N2 = 1.0f;

            // TODO: Optimize
            var containers = new List<Intersection>();
            foreach (var current in intersections)
            {
                var isCurrent = current == intersection;
                if (isCurrent && containers.Count > 0)
                {
                    N1 = containers[containers.Count - 1].Shape.Material.RefractiveIndex;
                }

                var removed = containers.RemoveAll(i => ReferenceEquals(current.Shape, i.Shape));
                if (removed == 0)
                {
                    containers.Add(current);
                }

                if (!isCurrent)
                {
                    continue;
                }

                if (containers.Count > 0)
                {
                    N2 = containers[containers.Count - 1].Shape.Material.RefractiveIndex;
                }

                break;
            }
        }

        public readonly float T;
        public readonly IShape Shape;
        public readonly Point Point;
        public readonly Point OverPoint;
        public readonly Point UnderPoint;
        public readonly Vector Eye;
        public readonly Vector Normal;
        public readonly bool IsInside;
        public readonly Vector Reflect;
        public readonly float N1;
        public readonly float N2;

        public const float Epsilon = 0.0001f;
    }
}