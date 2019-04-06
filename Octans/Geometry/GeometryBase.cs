namespace Octans.Geometry
{
    public abstract class GeometryBase : IGeometry
    {
        private Matrix _inverse;
        private Matrix _transform;

        protected GeometryBase()
        {
            Transform = Matrix.Identity;
            _inverse = Matrix.Identity;
            Material = new Material();
        }

        public Matrix Transform
        {
            get => _transform;
            protected set
            {
                _transform = value;
                _inverse = value.Inverse();
            }
        }

        public Matrix TransformInverse() => _inverse;

        public Material Material { get; protected set; }

        public abstract IIntersections LocalIntersects(in Ray localRay);

        public abstract Normal LocalNormalAt(in Point localPoint, in Intersection intersection);

        public void SetTransform(Matrix matrix)
        {
            // TODO: Allow mutations?
            Transform = matrix;
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