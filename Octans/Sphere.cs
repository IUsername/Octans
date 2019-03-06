using System;
using System.Collections.Generic;

namespace Octans
{
    public class Sphere
    {
        public Sphere()
        {
            Transform = Matrix.Identity;
        }

        public Matrix Transform { get; private set; }

        public IReadOnlyList<Intersection> Intersect(Ray ray)
        {
            var rt = ray.Transform(Transform.Inverse());
            // TODO: Only works for unit sphere on origin.
            var sphereToRay = rt.Origin - Point.Create(0, 0, 0);
            var a = Vector.Dot(rt.Direction, rt.Direction);
            var b = 2f * Vector.Dot(rt.Direction, sphereToRay);
            var c = Vector.Dot(sphereToRay, sphereToRay) - 1f;
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

        public Tuple NormalAt(Tuple worldPoint)
        {
            var inv = Transform.Inverse();
            var objPoint = inv * worldPoint;
            var objNorm = objPoint - Point.Create(0f, 0f, 0f);
            var worldNorm = inv.Transpose() * objNorm;
            worldNorm = Vector.Create(worldNorm.X, worldNorm.Y, worldNorm.Z);
            return worldNorm.Normalize();
        }
    }
}