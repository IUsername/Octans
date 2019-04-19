namespace Octans
{
    public interface IGeometry
    {
        MaterialInfo Material { get; }
        Transform Transform { get; }
        IGeometry Parent { get; set; }
        IIntersections LocalIntersects(in Ray localRay);
        Normal LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Transform transform);
        void SetMaterial(MaterialInfo material);
        Bounds LocalBounds();
        void Divide(int threshold);
    }
}