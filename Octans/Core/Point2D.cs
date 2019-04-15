﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Octans
{
    public readonly struct Point2D
    {
        public float X { get; }
        public float Y { get; }

        [DebuggerStepThrough]
        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        [Pure]
        public static Point2D Floor(in Point2D p)
        {
            return new Point2D(MathF.Floor(p.X), MathF.Floor(p.Y));
        }

        [Pure]
        public static Point2D Ceiling(in Point2D p)
        {
            return new Point2D(MathF.Ceiling(p.X), MathF.Ceiling(p.Y));
        }

        [Pure]
        public static Point2D Add(in Point2D left, in Vector2 right) => new Point2D(left.X + right.X, left.Y + right.Y);

        [Pure]
        public static Point2D Subtract(in Point2D left, in Vector2 right) => new Point2D(left.X - right.X, left.Y - right.Y);

        [Pure]
        public static Point2D operator +(Point2D left, Vector2 right) => Add(in left, in right);

        [Pure]
        public static Point2D operator -(Point2D left, Vector2 right) => Subtract(in left, in right);

        [Pure]
        public static explicit operator Point2D(in UVPoint uv) => new Point2D(uv.U, uv.V);

        [Pure]
        public static explicit operator Point2D(in PixelCoordinate pixel) => new Point2D(pixel.X, pixel.Y);
    }
}