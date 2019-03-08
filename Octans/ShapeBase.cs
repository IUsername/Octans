using System.Collections.Generic;

namespace Octans
{
    public abstract class ShapeBase : IShape
    {
        protected ShapeBase()
        {
            Transform = Matrix.Identity;
            Material = new Material();
        }

        public Matrix Transform { get; protected set; }

        public Material Material { get; protected set; }

        public abstract IReadOnlyList<Intersection> LocalIntersects(in Ray localRay);

        public abstract Vector LocalNormalAt(in Point localPoint);

        public void SetTransform(Matrix matrix)
        {
            // TODO: Allow mutations?
            Transform = matrix;
        }

        public void SetMaterial(Material material)
        {
            Material = material;
        }
    }
}