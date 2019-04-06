﻿using System;
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
        public static (float u, float v) Spherical(in Point p)
        {
            var n = new Vector(p.X, p.Y, p.Z).Normalize();
            var u = 1f - (MathF.Atan2(n.X, n.Z) / (2f * MathF.PI) + 0.5f);
            var v = 1f - MathF.Acos(n.Y) / MathF.PI;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) Planar(in Point point)
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

            return (u, v);
        }

        [Pure]
        public static (float u, float v) Cylindrical(in Point point)
        {
            var theta = MathF.Atan2(point.X, point.Z);
            var rawU = theta / (2f * MathF.PI);
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

            return (u, v);
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
        public static (float u, float v) Cubical(in Point point)
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
                    return (0f, 0f);
            }
        }

        [Pure]
        public static (float u, float v) SkyBox(in Point point)
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
                    var (fu,fv) = CubeUVFrontFace(in point);
                    return (fu *du + du, fv *dv + dv);
                case CubeFace.Back:
                    var (bu, bv) = CubeUVBackFace(in point);
                    return (bu*du + du3, bv*dv + dv);
                case CubeFace.Left:
                    var (lu, lv) = CubeUVLeftFace(in point);
                    return (lu*du, lv*dv + dv);
                case CubeFace.Right:
                    var (ru, rv) = CubeUVRightFace(in point);
                    return (ru *du + du2, rv*dv + dv);
                case CubeFace.Top:
                    var (tu, tv) = CubeUVTopFace(in point);
                    return (tu*du + du, tv* dv + dv2);
                case CubeFace.Bottom:
                    var (bou, bov) = CubeUVBottomFace(in point);
                    return (bou*du + du, bov*dv);
                default:
                    return (0f, 0f);
            }
        }

        [Pure]
        public static (float u, float v) CubeUVFrontFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) CubeUVBackFace(in Point point)
        {
            var u = (1f - point.X) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) CubeUVLeftFace(in Point point)
        {
            var u = (point.Z + 1f) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) CubeUVRightFace(in Point point)
        {
            var u = (1f - point.Z) % 2.0f / 2.0f;
            var v = (point.Y + 1f) % 2.0f / 2.0f;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) CubeUVTopFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (1f - point.Z) % 2.0f / 2.0f;
            return (u, v);
        }

        [Pure]
        public static (float u, float v) CubeUVBottomFace(in Point point)
        {
            var u = (point.X + 1f) % 2.0f / 2.0f;
            var v = (point.Z + 1f) % 2.0f / 2.0f;
            return (u, v);
        }
    }
}