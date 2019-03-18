using System;

namespace Octans
{
    public readonly struct Intersection : IEquatable<Intersection>, IComparable<Intersection>
    {
        public readonly float U;
        public readonly float V;
        public readonly float T;
        public readonly IShape Shape;

        public Intersection(float t, IShape shape) : this(t, shape, 0.0f, 0.0f)
        {
        }

        public Intersection(float t, IShape shape, float u, float v)
        {
            T = t;
            Shape = shape;
            U = u;
            V = v;
        }

        public bool Equals(Intersection other) => T.Equals(other.T)
                                                  && U.Equals(other.U) 
                                                  && V.Equals(other.V) 
                                                  && ReferenceEquals(Shape, other.Shape);

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
                var hashCode = U.GetHashCode();
                hashCode = (hashCode * 397) ^ V.GetHashCode();
                hashCode = (hashCode * 397) ^ T.GetHashCode();
                hashCode = (hashCode * 397) ^ (Shape != null ? Shape.GetHashCode() : 0);
                return hashCode;
            }
        }


        public static bool operator ==(Intersection left, Intersection right) => left.Equals(right);

        public static bool operator !=(Intersection left, Intersection right) => !left.Equals(right);

        public int CompareTo(Intersection other) => T.CompareTo(other.T);
    }
}