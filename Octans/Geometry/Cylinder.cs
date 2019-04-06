using System;

namespace Octans.Geometry
{
    public class Cylinder : GeometryBase
    {
        private const float Epsilon = 0.0001f;

        public Cylinder()
        {
            Minimum = float.NegativeInfinity;
            Maximum = float.PositiveInfinity;
            IsClosed = false;
        }

        public float Minimum { get; set; }
        public float Maximum { get; set; }
        public bool IsClosed { get; set; }

        public override IIntersections LocalIntersects(in Ray localRay)
        {
            var dX = localRay.Direction.X;
            var dZ = localRay.Direction.Z;
            var a = dX * dX + dZ * dZ;
            var builder = Intersections.Builder();

            if (!Check.Within(a, 0f, Epsilon))
            {
                var oX = localRay.Origin.X;
                var oZ = localRay.Origin.Z;
                var b = 2f * oX * dX + 2f * oZ * dZ;
                var c = oX * oX + oZ * oZ - 1f;
                var disc = b * b - 4f * a * c;
                if (disc < 0f)
                {
                    return builder.ToIntersections();
                }

                var t0 = (-b - MathF.Sqrt(disc)) / (2f * a);
                var t1 = (-b + MathF.Sqrt(disc)) / (2f * a);

                if (t0 > t1)
                {
                    (t0, t1) = (t1, t0);
                }


                var y0 = localRay.Origin.Y + t0 * localRay.Direction.Y;
                if (Minimum < y0 && y0 < Maximum)
                {
                    builder.Add(new Intersection(t0, this));
                }

                var y1 = localRay.Origin.Y + t1 * localRay.Direction.Y;
                if (Minimum < y1 && y1 < Maximum)
                {
                    builder.Add(new Intersection(t1, this));
                }
            }

            IntersectCaps(this, localRay, builder);

            return builder.ToIntersections();
        }

        private static bool CheckCap(Ray ray, float t)
        {
            var x = ray.Origin.X + t * ray.Direction.X;
            var z = ray.Origin.Z + t * ray.Direction.Z;
            var check = x * x + z * z;
            return check <= 1f || Check.Within(check, 1f, Epsilon);
        }

        private static void IntersectCaps(Cylinder cylinder, Ray ray, IIntersectionsBuilder list)
        {
            if (!cylinder.IsClosed || Check.Within(ray.Direction.Y, 0f, Epsilon) || list.Count == 2)
            {
                return;
            }

            var t = (cylinder.Minimum - ray.Origin.Y) / ray.Direction.Y;
            if (CheckCap(ray, t))
            {
                list.Add(new Intersection(t, cylinder));
            }

            t = (cylinder.Maximum - ray.Origin.Y) / ray.Direction.Y;
            if (CheckCap(ray, t) && list.Count < 2)
            {
                list.Add(new Intersection(t, cylinder));
            }
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection)
        {
            var dist = localPoint.X * localPoint.X + localPoint.Z * localPoint.Z;
            if (dist < 1f)
            {
                if (localPoint.Y >= Maximum - Epsilon)
                {
                    return new Normal(0, 1, 0);
                }

                if (localPoint.Y <= Minimum + Epsilon)
                {
                    return new Normal(0, -1, 0);
                }
            }

            return new Normal(localPoint.X, 0, localPoint.Z);
        }

        public override Bounds LocalBounds() => new Bounds(new Point(-1, Minimum, -1), new Point(1, Maximum, 1));
    }
}