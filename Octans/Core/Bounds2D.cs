using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Bounds2D
    {
        public Point2D Min { get; }
        public Point2D Max { get; }

        [DebuggerStepThrough]
        public Bounds2D(in Point2D min, in Point2D max)
        {
            Min = min;
            Max = max;
        }

        [DebuggerStepThrough]
        public Bounds2D(in float minX, in float minY, in float maxX, in float maxY)
        {
            Min = new Point2D(minX, minY);
            Max = new Point2D(maxX, maxY);
        }

        [Pure]
        public static explicit operator Bounds2D(in PixelArea area) =>
            new Bounds2D((Point2D) area.Min, (Point2D) area.Max);
    }
}