using System.Collections.Generic;

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

        public static Point ToLocal(in this Point worldPoint, IShape shape)
        {
            return shape.Transform.Inverse() * worldPoint;
        }

        public static Vector NormalAt(this IShape shape, in Point worldPoint)
        {
            var inv = shape.Transform.Inverse();
            var localPoint = inv * worldPoint;
            var localNormal = shape.LocalNormalAt(localPoint);
            var worldNormal = inv.Transpose() * localNormal;
            return worldNormal.ZeroW().Normalize();
        }
    }
}