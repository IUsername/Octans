using System.Diagnostics.Contracts;
using static System.MathF;
using static Octans.MathF;

namespace Octans
{
    public partial class Transform
    {
        public static Transform Identity = new Transform(Matrix.Identity, Matrix.Identity);

        public Transform(in Matrix matrix, in Matrix inverse)
        {
            Matrix = matrix;
            Inverse = inverse;
        }

        public Transform(in Matrix matrix)
        {
            Matrix = matrix;
            Inverse = matrix.Inverse();
        }

        public Matrix Matrix { get; }

        public Matrix Inverse { get; }

        [Pure]
        public bool SwapsHandedness()
        {
            var m = Matrix;
            var det = m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
                      m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
                      m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
            return det < 0f;
        }

        [Pure]
        public static Point Apply(in Transform t, in Point p)
        {
            var m = t.Matrix;
            var x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z + m[0, 3];
            var y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z + m[1, 3];
            var z = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z + m[2, 3];
            var w = m[3, 0] * p.X + m[3, 1] * p.Y + m[3, 2] * p.Z + m[3, 3];

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return w != 1f ? new Point(x, y, z) / w : new Point(x, y, z);
        }

        public static Point Apply(in Transform t, in Point p, out Vector pError)
        {
            var m = t.Matrix;
            var x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z + m[0, 3];
            var y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z + m[1, 3];
            var z = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z + m[2, 3];
            var w = m[3, 0] * p.X + m[3, 1] * p.Y + m[3, 2] * p.Z + m[3, 3];

            var xAbsSum = Abs(m[0, 0] * x) + Abs(m[0, 1] * y) +
                          Abs(m[0, 2] * z) + Abs(m[0, 3]);
            var yAbsSum = Abs(m[1, 0] * x) + Abs(m[1, 1] * y) +
                          Abs(m[1, 2] * z) + Abs(m[1, 3]);
            var zAbsSum = Abs(m[2, 0] * x) + Abs(m[2, 1] * y) +
                          Abs(m[2, 2] * z) + Abs(m[2, 3]);
            pError = Gamma(3) * new Vector(xAbsSum, yAbsSum, zAbsSum);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return w != 1f ? new Point(x, y, z) / w : new Point(x, y, z);
        }

        public static Point Apply(in Transform t, in Point p, in Vector ptError, out Vector absError)
        {
            var m = t.Matrix;
            var x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2] * p.Z + m[0, 3];
            var y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2] * p.Z + m[1, 3];
            var z = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2] * p.Z + m[2, 3];
            var w = m[3, 0] * p.X + m[3, 1] * p.Y + m[3, 2] * p.Z + m[3, 3];

            var xAbsError = (Gamma(3) + 1f) *
                            (Abs(m[0, 0] * ptError.X) + Abs(m[0, 1] * ptError.Y) +
                             Abs(m[0, 2] * ptError.Z) +
                             Gamma(3) * (Abs(m[0, 0] * p.X) + Abs(m[0, 1] * p.Y) +
                                         Abs(m[0, 2] * p.Z) + Abs(m[0, 3])));
            var yAbsError = (Gamma(3) + 1f) *
                            (Abs(m[1, 0] * ptError.X) + Abs(m[1, 1] * ptError.Y) +
                             Abs(m[1, 2] * ptError.Z) +
                             Gamma(3) * (Abs(m[1, 0] * p.X) + Abs(m[1, 1] * p.Y) +
                                         Abs(m[1, 2] * p.Z) + Abs(m[1, 3])));
            var zAbsError = (Gamma(3) + 1f) *
                            (Abs(m[2, 0] * ptError.X) + Abs(m[2, 1] * ptError.Y) +
                             Abs(m[2, 2] * ptError.Z) +
                             Gamma(3) * (Abs(m[2, 0] * p.X) + Abs(m[2, 1] * p.Y) +
                                         Abs(m[2, 2] * p.Z) + Abs(m[2, 3])));
            absError = new Vector(xAbsError, yAbsError, zAbsError);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return w != 1f ? new Point(x, y, z) / w : new Point(x, y, z);
        }

        [Pure]
        public static Point ApplyInverse(in Transform t, in Point p)
        {
            var i = t.Inverse;
            var x = i[0, 0] * p.X + i[0, 1] * p.Y + i[0, 2] * p.Z + i[0, 3];
            var y = i[1, 0] * p.X + i[1, 1] * p.Y + i[1, 2] * p.Z + i[1, 3];
            var z = i[2, 0] * p.X + i[2, 1] * p.Y + i[2, 2] * p.Z + i[2, 3];
            var w = i[3, 0] * p.X + i[3, 1] * p.Y + i[3, 2] * p.Z + i[3, 3];

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return w != 1f ? new Point(x, y, z) / w : new Point(x, y, z);
        }

        [Pure]
        private static Vector Apply(in Transform t, in Vector v)
        {
            var m = t.Matrix;
            var x = m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z;
            var y = m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z;
            var z = m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z;

            return new Vector(x, y, z);
        }

        [Pure]
        public static Vector Apply(in Transform t, in Vector v, out Vector absError)
        {
            var m = t.Matrix;
            var x = m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z;
            var y = m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z;
            var z = m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z;

            var xAbsError = Abs(m[0, 0] * v.X) + Abs(m[0, 1] * v.Y) + Abs(m[0, 2] * v.Z);
            var yAbsError = Abs(m[1, 0] * v.X) + Abs(m[1, 1] * v.Y) + Abs(m[1, 2] * v.Z);
            var zAbsError = Abs(m[2, 0] * v.X) + Abs(m[2, 1] * v.Y) + Abs(m[2, 2] * v.Z);

            absError = Gamma(3) * new Vector(xAbsError, yAbsError, zAbsError);

            return new Vector(x, y, z);
        }

        [Pure]
        private static Vector ApplyInverse(in Transform t, in Vector v)
        {
            var i = t.Inverse;
            var x = i[0, 0] * v.X + i[0, 1] * v.Y + i[0, 2] * v.Z;
            var y = i[1, 0] * v.X + i[1, 1] * v.Y + i[1, 2] * v.Z;
            var z = i[2, 0] * v.X + i[2, 1] * v.Y + i[2, 2] * v.Z;

            return new Vector(x, y, z);
        }

        [Pure]
        private static Normal Apply(in Transform t, in Normal n)
        {
            // Inverse transpose
            var i = t.Inverse;
            var x = i[0, 0] * n.X + i[1, 0] * n.Y + i[2, 0] * n.Z;
            var y = i[0, 1] * n.X + i[1, 1] * n.Y + i[2, 1] * n.Z;
            var z = i[0, 2] * n.X + i[1, 2] * n.Y + i[2, 2] * n.Z;

            return new Normal(x, y, z);
        }

        [Pure]
        private static Ray Apply(in Transform t, in Ray r)
        {
            var o = Apply(t, r.Origin, out var oError);
            var d = Apply(t, r.Direction);
            var len2 = d.LengthSquared();
            var tMax = r.TMax;
            if (len2 > 0)
            {
                var dt = (Vector.Abs(in d) % oError) / len2;
                o += d * dt;
                tMax -= dt;
            }

            return new Ray(o, d, tMax);
        }

        public static void ApplyInPlace(in Transform t, in Ray r)
        {
            var o = Apply(t, r.Origin, out var oError);
            var d = Apply(t, r.Direction);
            var len2 = d.LengthSquared();
            var tMax = r.TMax;
            if (len2 > 0)
            {
                var dt = (Vector.Abs(in d) % oError) / len2;
                o += d * dt;
                tMax -= dt;
            }

            r.Origin = o;
            r.Direction = d;
            r.InverseDirection = 1f / d;
            r.TMax = tMax;
        }

        [Pure]
        private static RayDifferential Apply(in Transform t, in RayDifferential r)
        {
            var tr = Apply(t, (Ray) r);
            var rd = new RayDifferential(tr)
            {
                HasDifferentials = r.HasDifferentials,
                RxOrigin = t * r.RxOrigin,
                RyOrigin = t * r.RyOrigin,
                RxDirection = t * r.RxDirection,
                RyDirection = t * r.RyDirection
            };
            return rd;
        }

      
        public static void ApplyInPlace(in Transform t, in RayDifferential r)
        {
          //  var hasDiff = r.HasDifferentials;
            ApplyInPlace(t, (Ray)r);
         //   r.HasDifferentials = hasDiff;
            if (r.HasDifferentials)
            {
                r.RxOrigin = t * r.RxOrigin;
                r.RyOrigin = t * r.RyOrigin;
                r.RxDirection = t * r.RxDirection;
                r.RyDirection = t * r.RyDirection;
            }
        }

        [Pure]
        public static Ray Apply(in Transform t, in Ray r, out Vector oError, out Vector dError)
        {
            var o = Apply(t, r.Origin, out oError);
            var d = Apply(t, r.Direction, out dError);
            var len2 = d.LengthSquared();
            var tMax = r.TMax;
            if (len2 > 0)
            {
                var dt = (Vector.Abs(in d) % oError) / len2;
                o += d * dt;
                //tMax -= dt;
            }

            return new Ray(o, d, tMax);
        }

        [Pure]
        public static SurfaceInteraction Apply(in Transform t, in SurfaceInteraction si)
        {
            var ret = new SurfaceInteraction();
            ret.Initialize(in si, in t);
            return ret;
        }

        [Pure]
        private static Ray ApplyInverse(in Transform t, in Ray r)
        {
            // TODO: Does not account for error.
            var o = t ^ r.Origin;
            var d = t ^ r.Direction;
            return new Ray(o, d, r.TMax);
        }

        [Pure]
        private static Bounds Apply(in Transform t, in Bounds b)
        {
            // Vectorize?
            var p = Bounds.ToCornerPoints(in b);
            p[0] = t * p[0];
            p[1] = t * p[1];
            p[2] = t * p[2];
            p[3] = t * p[3];
            p[4] = t * p[4];
            p[5] = t * p[5];
            p[6] = t * p[6];
            p[7] = t * p[7];
            return Bounds.FromPoints(p);
        }

        [Pure]
        public static Transform Multiply(in Transform left, in Transform right) =>
            new Transform(left.Matrix * right.Matrix, right.Inverse * left.Inverse);

        [Pure]
        public static Transform Invert(in Transform transform) => new Transform(transform.Inverse, transform.Matrix);

        [Pure]
        public static Transform Transpose(in Transform transform) =>
            new Transform(transform.Matrix.Transpose(), transform.Inverse.Transpose());

        [Pure]
        public static Transform operator *(Transform left, Transform right) => Multiply(in left, in right);

        [Pure]
        public static Point operator *(Transform left, Point right) => Apply(in left, in right);

        [Pure]
        public static SurfaceInteraction operator *(Transform left, SurfaceInteraction right) =>
            Apply(in left, in right);

        [Pure]
        public static Point operator ^(Transform left, Point right) => ApplyInverse(in left, in right);

        [Pure]
        public static Vector operator *(Transform left, Vector right) => Apply(in left, in right);

        [Pure]
        public static Vector operator ^(Transform left, Vector right) => ApplyInverse(in left, in right);

        [Pure]
        public static Normal operator *(Transform left, Normal right) => Apply(in left, in right);

        [Pure]
        public static Ray operator *(Transform left, Ray right) => Apply(in left, in right);

        [Pure]
        public static RayDifferential operator *(Transform left, RayDifferential right) => Apply(in left, in right);

        [Pure]
        public static Ray operator ^(Transform left, Ray right) => ApplyInverse(in left, in right);

        [Pure]
        public static (Ray r, Vector oError, Vector dError) operator &(Transform left, Ray right)
        {
            var r = Apply(in left, in right, out var oError, out var dError);
            return (r, oError, dError);
        }

        [Pure]
        public static Bounds operator *(Transform left, Bounds right) => Apply(in left, in right);
    }
}