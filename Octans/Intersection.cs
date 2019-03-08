using System;
using System.Collections.Generic;

namespace Octans
{
    public readonly struct Intersection : IEquatable<Intersection>
    {
        public readonly float T;
        public readonly IShape Shape;

        public Intersection(float t, IShape shape)
        {
            T = t;
            Shape = shape;
        }

        public bool Equals(Intersection other) => T.Equals(other.T) && ReferenceEquals(Shape, other.Shape);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Intersection other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (T.GetHashCode() * 397) ^ (Shape != null ? Shape.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Intersection left, Intersection right) => left.Equals(right);

        public static bool operator !=(Intersection left, Intersection right) => !left.Equals(right);
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