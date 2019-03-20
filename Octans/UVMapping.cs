using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public static class UVMapping
    {
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
            return (u,v);
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
    }
}