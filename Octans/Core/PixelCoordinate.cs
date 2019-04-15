using System;
using System.Diagnostics.Contracts;
using System.Numerics;

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

        [Pure]
        public PixelCoordinate SetX(int x)
        {
            return new PixelCoordinate(x, Y);
        }

        [Pure]
        public PixelCoordinate SetY(int y)
        {
            return new PixelCoordinate(X,y);
        }

        [Pure]
        public static PixelVector Subtract(in PixelCoordinate left, in PixelCoordinate right)
        {
            return new PixelVector(left.X - right.X, left.Y - right.Y);
        }

        [Pure]
        public static PixelCoordinate Add(in PixelCoordinate left, in PixelVector right)
        {
            return new PixelCoordinate(left.X + right.X, left.Y + right.Y);
        }

        [Pure]
        public static PixelVector operator -(PixelCoordinate left, PixelCoordinate right) => Subtract(in left, in right);

        [Pure]
        public static PixelCoordinate operator +(PixelCoordinate left, PixelVector right) => Add(in left, in right);

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

        [Pure]
        public static explicit operator Vector2(in PixelCoordinate pixel) => new Vector2(pixel.X, pixel.Y);

        [Pure]
        public static explicit operator PixelCoordinate(in Point2D pixel) =>
            new PixelCoordinate((int) pixel.X, (int) pixel.Y);

        [Pure]
        public static bool operator ==(PixelCoordinate left, PixelCoordinate right) => left.Equals(right);

        [Pure]
        public static bool operator !=(PixelCoordinate left, PixelCoordinate right) => !left.Equals(right);

        [Pure]
        public static PixelCoordinate Max(in PixelCoordinate a, in PixelCoordinate b)
        {
            return new PixelCoordinate(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        [Pure]
        public static PixelCoordinate Min(in PixelCoordinate a, in PixelCoordinate b)
        {
            return new PixelCoordinate(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }
    }
}