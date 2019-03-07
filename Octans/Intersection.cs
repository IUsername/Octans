using System.Collections.Generic;

namespace Octans
{
    public readonly struct Intersection
    {
        public readonly float T;
        public readonly IShape Shape;

        public Intersection(float t, IShape shape)
        {
            T = t;
            Shape = shape;
        }
    }

    public static class IntersectionExtensions
    {
        public static Intersection? Hit(this IReadOnlyList<Intersection> intersections)
        {
            for (var i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].T > 0)
                {
                    return intersections[i];
                }
            }
            return null;
        }
    }
}