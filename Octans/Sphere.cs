using System;
using System.Collections.Generic;

namespace Octans
{
    public class Sphere : ISurface
    {
        public Sphere()
        {
            Transform = Matrix.Identity;
            Material = new Material();
        }

        public Matrix Transform { get; private set; }

        public Material Material { get; private set; }

        public IReadOnlyList<Intersection> Intersect(Ray ray)
        {
            var rt = ray.Transform(Transform.Inverse());
            // TODO: Only works for unit sphere on origin.
            var sphereToRay = rt.Origin - Point.Zero;
            var a = rt.Direction % rt.Direction;
            var b = 2f * rt.Direction % sphereToRay;
            var c = sphereToRay % sphereToRay - 1f;
            var discriminant = b * b - 4 * a * c;
            if (discriminant < 0f)
            {
                return Intersections.Empty;
            }

            var t1 = (-b - MathF.Sqrt(discriminant)) / (2f * a);
            var t2 = (-b + MathF.Sqrt(discriminant)) / (2f * a);
            return new Intersections(
                new Intersection(t1, this),
                new Intersection(t2, this));
        }

        public void SetTransform(in Matrix matrix)
        {
            // TODO: Allow mutations?
            Transform = matrix;
        }

        public void SetMaterial(Material material)
        {
            Material = material;
        }

        public Vector NormalAt(Point world)
        {
            var inv = Transform.Inverse();
            var objPoint = inv * world;
            var objNorm = objPoint - Point.Zero;
            var worldNorm = inv.Transpose() * objNorm;
            worldNorm = new Vector(worldNorm.X, worldNorm.Y, worldNorm.Z);
            return worldNorm.Normalize();
        }
    }
}