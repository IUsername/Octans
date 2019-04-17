using Octans.Material;
using Octans.Memory;
using Octans.Reflection;

namespace Octans.Geometry
{
    public abstract class GeometryBase : IGeometry
    {
     //   private Matrix _inverse;
     //    private Matrix _inverseTranspose;

        protected GeometryBase()
        {
            Transform = Transform.Identity;
      //      _inverse = Matrix.Identity;
     //       _inverseTranspose = Matrix.Identity;
            Material = new MaterialInfo();
        }

        public Transform Transform { get; protected set; }

        //  public Matrix TransformInverse() => _inverse;
      //  public Matrix TransformInverseTranspose() => _inverseTranspose;

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
    }
}