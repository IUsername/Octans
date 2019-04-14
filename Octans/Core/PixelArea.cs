using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct PixelArea
    {
        public PixelCoordinate Min { get; }
        public PixelCoordinate Max { get; }

        public PixelArea(PixelCoordinate min, PixelCoordinate max)
        {
            Min = min;
            Max = max;
        }

        [Pure]
        public int Area() => (Max.X - Min.X) * (Max.Y - Min.Y);

        [Pure]
        public static explicit operator PixelArea(in Bounds2D b) =>
            new PixelArea(
                (PixelCoordinate) Point2D.Floor(b.Min),
                (PixelCoordinate) Point2D.Ceiling(b.Max));

        [Pure]
        public static PixelArea Intersect(in PixelArea a, in PixelArea b)
        {
            var xMin = Math.Max(a.Min.X, b.Min.X);
            var yMin = Math.Max(a.Min.Y, b.Min.Y);
            var xMax = Math.Min(a.Max.X, b.Max.X);
            var yMax = Math.Min(a.Max.Y, b.Max.Y);
            return new PixelArea(new PixelCoordinate(xMin, yMin), new PixelCoordinate(xMax, yMax));
        }
    }
}