using System;

namespace Octans
{
    public readonly struct Intersection : IEquatable<Intersection>, IComparable<Intersection>
    {
        public readonly float U;
        public readonly float V;
        public readonly float T;
        public readonly IGeometry Geometry;

        public Intersection(float t, IGeometry geometry) : this(t, geometry, 0.0f, 0.0f)
        {
        }

        public Intersection(float t, IGeometry geometry, float u, float v)
        {
            T = t;
            Geometry = geometry;
            U = u;
            V = v;
        }

        public bool Equals(Intersection other) => T.Equals(other.T)
                                                  && U.Equals(other.U)
                                                  && V.Equals(other.V)
                                                  && ReferenceEquals(Geometry, other.Geometry);

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
                hashCode = (hashCode * 397) ^ (Geometry != null ? Geometry.GetHashCode() : 0);
                return hashCode;
            }
        }


        public static bool operator ==(Intersection left, Intersection right) => left.Equals(right);

        public static bool operator !=(Intersection left, Intersection right) => !left.Equals(right);

        public int CompareTo(Intersection other) => T.CompareTo(other.T);
    }
}