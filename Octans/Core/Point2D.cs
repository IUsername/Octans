using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct Point2D
    {
        public float X { get; }
        public float Y { get; }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        [Pure]
        public static Point2D Add(in Point2D left, in Point2D right) => new Point2D(left.X + right.X, left.Y + right.Y);

        [Pure]
        public static Point2D operator +(Point2D left, Point2D right) => Add(in left, in right);

        [Pure]
        public static explicit operator Point2D(in UVPoint uv) => new Point2D(uv.U, uv.V);

        [Pure]
        public static explicit operator Point2D(in PixelCoordinate pixel) => new Point2D(pixel.X, pixel.Y);
    }
}