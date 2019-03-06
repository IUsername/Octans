using System.Collections.Generic;

namespace Octans
{
    public readonly struct Intersection
    {
        public readonly float T;
        public readonly object Obj;

        public Intersection(float t, object obj)
        {
            T = t;
            Obj = obj;
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