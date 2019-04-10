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
            Material = new Material();
        }

        public Transform Transform { get; protected set; }

        //  public Matrix TransformInverse() => _inverse;
      //  public Matrix TransformInverseTranspose() => _inverseTranspose;

        public Material Material { get; protected set; }

        public abstract IIntersections LocalIntersects(in Ray localRay);

        public abstract Normal LocalNormalAt(in Point localPoint, in Intersection intersection);

        public void SetTransform(Transform transform)
        {
            // TODO: Allow mutations?
            Transform = transform;
        }

        public void SetMaterial(Material material)
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
    }
}