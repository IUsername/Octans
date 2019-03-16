using System;
using System.Collections.Generic;

namespace Octans
{
    public class Triangle : ShapeBase
    {
        private const float Epsilon = 0.0001f;

        public Triangle(Point p1, Point p2, Point p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            E1 = p2 - p1;
            E2 = p3 - p1;
            Normal = Vector.Cross(E2, E1).Normalize();
        }

        public Point P1 { get; }
        public Point P2 { get; }
        public Point P3 { get; }

        public Vector Normal { get; }

        public Vector E2 { get; }

        public Vector E1 { get; }

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            // Möller–Trumbore algorithm.
            // www.tandfonline.com/doi/abs/10.1080/10867651.1997.10487468 
            var dirCrossE2 = Vector.Cross(localRay.Direction, E2);
            var det = E1 % dirCrossE2;
            if (MathF.Abs(det) < Epsilon)
            {
                return Intersections.Empty();
            }

            var f = 1f / det;
            var p1ToOrigin = localRay.Origin - P1;
            var u = f * p1ToOrigin % dirCrossE2;
            if (u < 0f || u > 1f)
            {
                return Intersections.Empty();
            }

            var originCrossE1 = Vector.Cross(p1ToOrigin, E1);
            var v = f * localRay.Direction % originCrossE1;
            if (v < 0 || u + v > 1)
            {
                return Intersections.Empty();
            }

            var t = f * E2 % originCrossE1;
            return Intersections.Create(new Intersection(t, this));
        }

        public override Vector LocalNormalAt(in Point localPoint) => Normal;

        public override Bounds LocalBounds()
        {
            return Bounds.FromPoints(new[] {P1, P2, P3});
        }
    }
}