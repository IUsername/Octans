using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using static System.Math;

namespace Octans
{
    public readonly struct PixelArea : IEnumerable<PixelCoordinate>, IEquatable<PixelArea>
    {
        public PixelCoordinate Min { get; }
        public PixelCoordinate Max { get; }

        public PixelArea(PixelCoordinate min, PixelCoordinate max)
        {
            Min = min;
            Max = max;
        }

        public PixelArea(int xMin, int yMin, int xMax, int yMax)
        {
            Min = new PixelCoordinate(xMin, yMin);
            Max = new PixelCoordinate(xMax, yMax);
        }

        [Pure]
        public int Area() => (Max.X - Min.X) * (Max.Y - Min.Y);

        [Pure]
        public bool InsideExclusive(in PixelCoordinate p) => p.X >= Min.X && p.X < Max.X && p.Y >= Min.Y && p.Y < Max.Y;

        [Pure]
        public static explicit operator PixelArea(in Bounds2D b) =>
            new PixelArea(
                (PixelCoordinate) Point2D.Floor(b.Min),
                (PixelCoordinate) Point2D.Ceiling(b.Max));

        [Pure]
        public static PixelArea Intersect(in PixelArea a, in PixelArea b)
        {
            var xMin = Max(a.Min.X, b.Min.X);
            var yMin = Max(a.Min.Y, b.Min.Y);
            var xMax = Min(a.Max.X, b.Max.X);
            var yMax = Min(a.Max.Y, b.Max.Y);
            return new PixelArea(new PixelCoordinate(xMin, yMin), new PixelCoordinate(xMax, yMax));
        }

        public IEnumerator<PixelCoordinate> GetEnumerator()
        {
            for (var y = Min.Y; y < Max.Y; ++y)
            {
                for (var x = Min.X; x < Max.X; ++x)
                {
                    yield return new PixelCoordinate(x, y);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(PixelArea other) => Min.Equals(other.Min) && Max.Equals(other.Max);

        public override bool Equals(object obj) => obj is PixelArea other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min.GetHashCode() * 397) ^ Max.GetHashCode();
            }
        }

        public static bool operator ==(PixelArea left, PixelArea right) => left.Equals(right);

        public static bool operator !=(PixelArea left, PixelArea right) => !left.Equals(right);
    }
}