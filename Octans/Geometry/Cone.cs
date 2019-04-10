using System;

namespace Octans.Geometry
{
    public class Cone : GeometryBase
    {
        private const float Epsilon = 0.00001f;

        public Cone()
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
            var dY = localRay.Direction.Y;
            var dZ = localRay.Direction.Z;

            var oX = localRay.Origin.X;
            var oY = localRay.Origin.Y;
            var oZ = localRay.Origin.Z;

            var a = dX * dX - dY * dY + dZ * dZ;
            var b = 2f * (oX * dX - oY * dY + oZ * dZ);
            var c = oX * oX - oY * oY + oZ * oZ;

            var builder = Intersections.Builder();

            if (Check.Within(a, 0f, Epsilon))
            {
                if (Check.Within(b, 0f, Epsilon))
                {
                    return builder.ToIntersections();
                }

                var t = -c / (2f * b);
                builder.Add(new Intersection(t, this));
            }
            else
            {
                var disc = b * b - 4f * a * c;
                if (disc < 0f)
                {
                    if (disc < -Epsilon / 2f)
                    {
                        return builder.ToIntersections();
                    }

                    //// Corner case where ray will intersect at one
                    //// point without passing through.
                    disc = 0;
                }

                var sqrt = MathF.Sqrt(disc);
                var t0 = (-b - sqrt) / (2f * a);
                var t1 = (-b + sqrt) / (2f * a);

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
            var r = MathF.Abs(ray.Origin.Y + t * ray.Direction.Y);
            var check = x * x + z * z;
            return check <= r || Check.Within(check, r, Epsilon);
        }

        private static void IntersectCaps(Cone cone, Ray ray, IIntersectionsBuilder builder)
        {
            if (!cone.IsClosed)
            {
                return;
            }

            var t = (cone.Minimum - ray.Origin.Y) / ray.Direction.Y;
            if (CheckCap(ray, t))
            {
                builder.Add(new Intersection(t, cone));
            }

            t = (cone.Maximum - ray.Origin.Y) / ray.Direction.Y;
            if (CheckCap(ray, t))
            {
                builder.Add(new Intersection(t, cone));
            }
        }

        private static bool WithinRadius(in Point localPoint)
        {
            var dist = localPoint.X * localPoint.X + localPoint.Z * localPoint.Z;
            var r = localPoint.Y * localPoint.Y;
            return dist < r;
        }

        public override Normal LocalNormalAt(in Point localPoint, in Intersection intersection)
        {
            if (IsClosed)
            {
                if (localPoint.Y >= Maximum - Epsilon)
                {
                    if (WithinRadius(in localPoint))
                    {
                        return Normals.YPos;
                    }
                }
                else if (localPoint.Y <= Minimum + Epsilon)
                {
                    if (WithinRadius(in localPoint))
                    {
                        return Normals.YNeg;
                    }
                }
            }

            var y = MathF.Sqrt(localPoint.X * localPoint.X + localPoint.Z * localPoint.Z);
            if (localPoint.Y > 0)
            {
                y = -y;
            }

            return new Normal(localPoint.X, y, localPoint.Z);
        }

        public override Bounds LocalBounds()
        {
            if (IsClosed)
            {
                var xz = MathF.Max(MathF.Acos(Minimum), Maximum);
                return new Bounds(new Point(-xz, Minimum, -xz), new Point(xz, Maximum, xz));
            }
            else
            {
                return Bounds.Infinite;
            }
        }
    }
}