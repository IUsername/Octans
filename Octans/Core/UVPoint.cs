using System;
using System.Diagnostics.Contracts;

namespace Octans
{
    public readonly struct UVPoint
    {
        public float U { get; }
        public float V { get; }

        public UVPoint(float u, float v)
        {
            U = u;
            V = v;
        }

        [Pure]
        public static UVPoint Scale(in UVPoint p, in float scalar) => new UVPoint(p.U * scalar, p.V * scalar);


        [Pure]
        public static UVPoint operator *(UVPoint t, float scalar) => Scale(in t, in scalar);
    }
}