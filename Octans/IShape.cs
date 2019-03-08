using System.Collections.Generic;

namespace Octans
{
    public interface IShape
    {
        IReadOnlyList<Intersection> LocalIntersects(in Ray localRay);
        Material Material { get; }
        Matrix Transform { get; }
        Vector LocalNormalAt(in Point localPoint);
        void SetTransform(Matrix matrix);
        void SetMaterial(Material material);
    }
}