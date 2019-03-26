namespace Octans
{
    public interface IGeometry
    {
        Material Material { get; }
        Matrix Transform { get; }

        IGeometry Parent { get; set; }
        IIntersections LocalIntersects(in Ray localRay);

        Matrix TransformInverse();
        Vector LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Matrix matrix);
        void SetMaterial(Material material);
        Bounds LocalBounds();
        void Divide(int threshold);
    }
}