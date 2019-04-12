using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct PixelCoordinate : IEquatable<PixelCoordinate>
    {
        public int X { get; }
        public int Y { get; }

        public PixelCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public PixelCoordinate SetX(int x)
        {
            return new PixelCoordinate(x, Y);
        }

        public PixelCoordinate SetY(int y)
        {
            return new PixelCoordinate(X,y);
        }

        public static PixelVector Subtract(in PixelCoordinate left, in PixelCoordinate right)
        {
            return new PixelVector(left.X - right.X, left.Y - right.Y);
        }

        [Pure]
        public static PixelVector operator -(PixelCoordinate left, PixelCoordinate right) => Subtract(in left, in right);

        public bool Equals(PixelCoordinate other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is PixelCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(PixelCoordinate left, PixelCoordinate right) => left.Equals(right);

        public static bool operator !=(PixelCoordinate left, PixelCoordinate right) => !left.Equals(right);
    }
}