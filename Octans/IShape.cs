
namespace Octans
{
    public interface IShape
    {
        IIntersections LocalIntersects(in Ray localRay);
        Material Material { get; }
        Matrix Transform { get; }

        Matrix TransformInverse();

        IShape Parent { get; set; }
        Vector LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Matrix matrix);
        void SetMaterial(Material material);
        Bounds LocalBounds();
        void Divide(int threshold);
    }
}