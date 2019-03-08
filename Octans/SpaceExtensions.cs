using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Octans
{
    public static class SpaceExtensions
    {
        public static Ray ToLocal(in this Ray worldRay, IShape shape)
        {
            return worldRay.Transform(shape.Transform.Inverse());
        }
        
        public static IReadOnlyList<Intersection> Intersects(this IShape shape, in Ray worldRay)
        {
            var localRay = worldRay.ToLocal(shape);
            return shape.LocalIntersects(in localRay);
        }

        [Pure]
        public static Point ToLocal(in this Point worldPoint, IShape shape)
        {
            return Matrix.Inverse(shape.Transform) * worldPoint;
        }

        [Pure]
        public static Point ToLocal(in this Point worldPoint, IShape shape, IPattern pattern)
        {
            var localPoint = worldPoint.ToLocal(shape);
            return Matrix.Inverse(pattern.Transform) * localPoint;
        }

        public static Vector NormalAt(this IShape shape, in Point worldPoint)
        {
            var inv = Matrix.Inverse(shape.Transform);
            var localPoint = inv * worldPoint;
            var localNormal = shape.LocalNormalAt(localPoint);
            var worldNormal = inv.Transpose() * localNormal;
            return worldNormal.ZeroW().Normalize();
        }
    }
}