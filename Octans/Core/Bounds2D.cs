using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Bounds2D
    {
        public Point2D Min { get; }
        public Point2D Max { get; }

        public Bounds2D(in Point2D min, in Point2D max)
        {
            Min = min;
            Max = max;
        }

        [Pure]
        public static explicit operator Bounds2D(in PixelArea area) =>
            new Bounds2D((Point2D) area.Min, (Point2D) area.Max);
    }
}