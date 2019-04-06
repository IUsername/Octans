using System.Diagnostics.Contracts;

namespace Octans
{
    public static class SpaceExtensions
    {
        [Pure]
        public static Ray ToLocal(in this Ray worldRay, in IGeometry geometry) =>
            worldRay.Transform(geometry.TransformInverse());

        [Pure]
        public static IIntersections Intersects(this IGeometry geometry, in Ray worldRay)
        {
            var localRay = worldRay.ToLocal(in geometry);
            return geometry.LocalIntersects(in localRay);
        }

        [Pure]
        public static Point ToLocal(this IGeometry geometry, in Point worldPoint)
        {
            var world = worldPoint;
            if (geometry.Parent != null)
            {
                world = geometry.Parent.ToLocal(in worldPoint);
            }

            return geometry.TransformInverse() * world;
        }

        [Pure]
        public static Point ToLocal(in this Point worldPoint, IGeometry geometry, ITexture texture)
        {
            var world = worldPoint;
            if (geometry.Parent != null)
            {
                world = geometry.Parent.ToLocal(in worldPoint);
            }

            var localPoint = geometry.ToLocal(in world);
            return texture.TransformInverse() * localPoint;
        }

        [Pure]
        public static Vector NormalAt(this IGeometry geometry, in Point worldPoint, in Intersection intersection)
        {
            var localPoint = geometry.ToLocal(in worldPoint);
            var localNormal = geometry.LocalNormalAt(localPoint, in intersection);
            return geometry.NormalToWorld(in localNormal);
        }

        [Pure]
        public static Vector NormalToWorld(this IGeometry geometry, in Vector localNormal)
        {
            var normal = geometry.TransformInverse().Transpose() * localNormal;
            normal = normal.Normalize();
            if (geometry.Parent != null)
            {
                normal = NormalToWorld(geometry.Parent, normal);
            }

            return normal;
        }

        [Pure]
        public static Bounds ParentSpaceBounds(this IGeometry geometry) => geometry.LocalBounds().Transform(geometry.Transform);
    }
}