namespace Octans
{
    public interface IShape
    {
        Material Material { get; }
        Matrix Transform { get; }

        IShape Parent { get; set; }
        IIntersections LocalIntersects(in Ray localRay);

        Matrix TransformInverse();
        Vector LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Matrix matrix);
        void SetMaterial(Material material);
        Bounds LocalBounds();
        void Divide(int threshold);
    }
}