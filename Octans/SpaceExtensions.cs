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
        public static Point ToLocal(this IShape shape, in Point worldPoint)
        {
            var world = worldPoint;
            if (shape.Parent != null)
            {
                world = shape.Parent.ToLocal(in worldPoint);
            }
            return Matrix.Inverse(shape.Transform) * world;
        }

        [Pure]
        public static Point ToLocal(in this Point worldPoint, IShape shape, IPattern pattern)
        {
            var world = worldPoint;
            if (shape.Parent != null)
            {
                world = shape.Parent.ToLocal(in worldPoint);
            }
            var localPoint = shape.ToLocal(in world);
            return Matrix.Inverse(pattern.Transform) * localPoint;
        }

        public static Vector NormalAt(this IShape shape, in Point worldPoint)
        {
            var localPoint = shape.ToLocal(in worldPoint);
            var localNormal = shape.LocalNormalAt(localPoint);
            return shape.NormalToWorld(in localNormal);
        }

        public static Vector NormalToWorld(this IShape shape, in Vector localNormal)
        {
            var normal = shape.Transform.Inverse().Transpose() * localNormal;
            normal = normal.ZeroW().Normalize();
            if(shape.Parent != null)
            {
                normal = NormalToWorld(shape.Parent, normal);
            }
            return normal;
        }
    }
}