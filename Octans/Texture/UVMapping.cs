using System;
using System.Diagnostics.Contracts;

namespace Octans.Texture
{
    public static class UVMapping
    {
        public enum CubeFace
        {
            Front,
            Left,
            Right,
            Top,
            Bottom,
            Back
        }

        [Pure]
        public static UVPoint Spherical(in Point p)
        {
            var n = new Vector(p.X, p.Y, p.Z).Normalize();
            var u = 1f - (System.MathF.Atan2(n.X, n.Z) / (2f * System.MathF.PI) + 0.5f);
            var v = 1f - System.MathF.Acos(n.Y) / System.MathF.PI;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint Planar(in Point point)
        {
            var u = point.X % 1.0f;
            var v = point.Z % 1.0f;
            if (u < 0f)
            {
                u = 1f + u;
            }

            if (v < 0f)
            {
                v = 1f + v;
            }

            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint Cylindrical(in Point point)
        {
            var theta = System.MathF.Atan2(point.X, point.Z);
            var rawU = theta / (2f * System.MathF.PI);
            var u = 1f - (rawU + 0.5f);
            var v = point.Y % 1f;

            if (u < 0f)
            {
                u = 1f + u;
            }

            if (v < 0f)
            {
                v = 1f + v;
            }

            return new UVPoint(u, v);
        }

        [Pure]
        public static CubeFace PointToCubeFace(in Point point)
        {
            var max = Point.Max(Point.Abs(in point));
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (max == point.X)
            {
                return CubeFace.Right;
            }

            if (max == -point.X)
            {
                return CubeFace.Left;
            }

            if (max == point.Y)
            {
                return CubeFace.Top;
            }

            if (max == -point.Y)
            {
                return CubeFace.Bottom;
            }

            if (max == point.Z)
            {
                return CubeFace.Front;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return CubeFace.Back;
        }

        [Pure]
        public static UVPoint Cubical(in Point point)
        {
            var face = PointToCubeFace(in point);
            switch (face)
            {
                case CubeFace.Front:
                    return CubeUVFrontFace(in point);
                case CubeFace.Back:
                    return CubeUVBackFace(in point);
                case CubeFace.Left:
                    return CubeUVLeftFace(in point);
                case CubeFace.Right:
                    return CubeUVRightFace(in point);
                case CubeFace.Top:
                    return CubeUVTopFace(in point);
                case CubeFace.Bottom:
                    return CubeUVBottomFace(in point);
                default:
                    return new UVPoint(0f, 0f);
            }
        }

        [Pure]
        public static UVPoint SkyBox(in Point point)
        {
            const float du = 0.25f;
            const float du2 = 0.5f;
            const float du3 = 0.75f;
            const float dv = 1 / 3f;
            const float dv2 = 2 / 3f;
            var face = PointToCubeFace(in point);
            switch (face)
            {
                // TODO: These may all be flipped vertically. Find better source.
                case CubeFace.Front:
                    var fuv = CubeUVFrontFace(in point);
                    return new UVPoint(fuv.U * du + du, fuv.V * dv + dv);
                case CubeFace.Back:
                    var buv = CubeUVBackFace(in point);
                    return new UVPoint(buv.U * du + du3, buv.V * dv + dv);
                case CubeFace.Left:
                    var luv = CubeUVLeftFace(in point);
                    return new UVPoint(luv.U * du, luv.V * dv + dv);
                case CubeFace.Right:
                    var ruv = CubeUVRightFace(in point);
                    return new UVPoint(ruv.U * du + du2, ruv.V * dv + dv);
                case CubeFace.Top:
                    var tuv = CubeUVTopFace(in point);
                    return new UVPoint(tuv.U * du + du, tuv.V * dv + dv2);
                case CubeFace.Bottom:
                    var bouv = CubeUVBottomFace(in point);
                    return new UVPoint(bouv.U * du + du, bouv.V * dv);
                default:
                    return new UVPoint(0f, 0f);
            }
        }

        [Pure]
        public static UVPoint CubeUVFrontFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint CubeUVBackFace(in Point point)
        {
            var u = (1f - point.X) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint CubeUVLeftFace(in Point point)
        {
            var u = (point.Z + 1f) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint CubeUVRightFace(in Point point)
        {
            var u = (1f - point.Z) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint CubeUVTopFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (1f - point.Z) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }

        [Pure]
        public static UVPoint CubeUVBottomFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (point.Z + 1f) % 2.0f / 2.0f;
            return new UVPoint(u, v);
        }
    }
}