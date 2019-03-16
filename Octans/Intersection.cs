using System;

namespace Octans
{
    public readonly struct Intersection : IEquatable<Intersection>, IComparable<Intersection>
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

        public int CompareTo(Intersection other) => T.CompareTo(other.T);
    }
}