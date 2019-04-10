namespace Octans
{
    public interface IGeometry
    {
        Material Material { get; }
        Transform Transform { get; }

        IGeometry Parent { get; set; }
        IIntersections LocalIntersects(in Ray localRay);

        //Matrix TransformInverse();
        //Matrix TransformInverseTranspose();
        Normal LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Transform transform);
        void SetMaterial(Material material);
        Bounds LocalBounds();
        void Divide(int threshold);
    }
}