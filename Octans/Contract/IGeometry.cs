namespace Octans
{
    public interface IGeometry
    {
        MaterialInfo Material { get; }
        Transform Transform { get; }

        IGeometry Parent { get; set; }

        // TODO: Rename after migration
        IMaterial Material2 { get; set; }

        IIntersections LocalIntersects(in Ray localRay);

        Normal LocalNormalAt(in Point localPoint, in Intersection intersection);
        void SetTransform(Transform transform);
        void SetMaterial(MaterialInfo material);
        Bounds LocalBounds();
        void Divide(int threshold);

        void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                        IObjectArena arena,
                                        TransportMode mode,
                                        in bool allowMultipleLobes);

        bool Intersect2(in Ray ray, ref SurfaceInteraction si);
        bool IntersectP(in Ray ray);
    }
}