using System;

namespace Octans.Geometry
{
    public abstract class GeometryBase : IGeometry
    {
        protected GeometryBase()
        {
            Transform = Transform.Identity;
            Material = new MaterialInfo();
        }

        public Transform Transform { get; protected set; }

        public MaterialInfo Material { get; protected set; }

        public abstract IIntersections LocalIntersects(in Ray localRay);

        public abstract Normal LocalNormalAt(in Point localPoint, in Intersection intersection);

        public void SetTransform(Transform transform)
        {
            // TODO: Allow mutations?
            Transform = transform;
        }

        public void SetMaterial(MaterialInfo material)
        {
            Material = material;
        }

        public IGeometry Parent { get; set; }
        public abstract Bounds LocalBounds();

        public virtual void Divide(int threshold)
        {
            // Do nothing
            // Should override to internally divide groups and CSG
        }

        // TODO: Rename after migration.
        public IMaterial Material2 { get; set; }

        public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               in bool allowMultipleLobes)
        {
            Material2?.ComputeScatteringFunctions(surfaceInteraction, arena, mode, allowMultipleLobes);
        }

        public bool Intersect2(in Ray ray, ref SurfaceInteraction si)
        {
            var localRay = ray.ToLocal(this);
            var li = LocalIntersects(in localRay);
            var intersection = li.Hit();
            if (intersection.HasValue)
            {
                var info = intersection.Value;
                var p = ray.Position(info.T);
                var n = info.Geometry.NormalAt(in p, in info);
                si.InitializeT(p, new Vector(), new Point2D(info.U, info.V), ray.Direction, n, new Vector(), new Vector(),
                              new Normal(), Normals.ZPos, info.Geometry);
                return true;
            }

            return false;
        }

        public bool IntersectP(in Ray ray) => throw new NotImplementedException();
    }
}