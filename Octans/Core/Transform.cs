using System;
using System.Diagnostics.Contracts;

namespace Octans.Core
{
    public class Transform
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
        public static Transform Multiply(in Transform left, in Transform right) =>
            new Transform(left.Matrix * right.Matrix, left.Inverse * right.Inverse);

        [Pure]
        public static Transform Invert(in Transform transform) => new Transform(transform.Inverse, transform.Matrix);

        [Pure]
        public static Transform Transpose(in Transform transform) =>
            new Transform(transform.Matrix.Transpose(), transform.Inverse.Transpose());

        [Pure]
        public static Transform operator *(Transform left, Transform right) => Multiply(in left, in right);


        public static Transform Translate(float x, float y, float z)
        {
            var m = new Matrix(
                1, 0, 0, x,
                0, 1, 0, y,
                0, 0, 1, z,
                0, 0, 0, 1);

            var mInv = new Matrix(
                1, 0, 0, -x,
                0, 1, 0, -y,
                0, 0, 1, -z,
                0, 0, 0, 1);

            return new Transform(m, mInv);
        }

        public static Transform TranslateX(float x) => Translate(x, 0f, 0f);
        public static Transform TranslateY(float y) => Translate(0f, y, 0f);
        public static Transform TranslateZ(float z) => Translate(0f, 0f, z);

        public static Transform Scale(float x, float y, float z)
        {
            var m = new Matrix(
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1);

            var mInv = new Matrix(
                1 / x, 0, 0, 0,
                0, 1 / y, 0, 0,
                0, 0, 1 / z, 0,
                0, 0, 0, 1);

            return new Transform(m, mInv);
        }

        public static Transform Scale(float factor) => Scale(factor, factor, factor);

        public static Transform RotateX(float rad)
        {
            var m = new Matrix(
                1, 0, 0, 0,
                0, MathF.Cos(rad), -MathF.Sin(rad), 0,
                0, MathF.Sin(rad), MathF.Cos(rad), 0,
                0, 0, 0, 1);

            return new Transform(m, m.Transpose());
        }

        public static Transform RotateY(float rad)
        {
            var m = new Matrix(
                MathF.Cos(rad), 0, MathF.Sin(rad), 0,
                0, 1, 0, 0,
                -MathF.Sin(rad), 0, MathF.Cos(rad), 0,
                0, 0, 0, 1);

            return new Transform(m, m.Transpose());
        }

        public static Transform RotateZ(float rad)
        {
            var m = new Matrix(
                MathF.Cos(rad), -MathF.Sin(rad), 0, 0,
                MathF.Sin(rad), MathF.Cos(rad), 0, 0,
                0.0f, 0, 1, 0,
                0.0f, 0, 0, 1);

            return new Transform(m, m.Transpose());
        }

        public static Transform Shear(float xy, float xz, float yx, float yz, float zx, float zy)
        {
            var m = new Matrix(1, xy, xz, 0,
                               yx, 1, yz, 0,
                               zx, zy, 1, 0,
                               0, 0, 0, 1);
            return new Transform(m);
        }

        public static Transform Rotate(float theta, in Vector axis)
        {
            var a = axis.Normalize();
            var sinTheta = MathF.Sin(theta);
            var cosTheta = MathF.Cos(theta);
            var oneMinusCos = 1f - cosTheta;
            var m = new Matrix(
                cosTheta + a.X * a.X * oneMinusCos,
                a.X * a.Y * oneMinusCos - a.Z * sinTheta,
                a.X * a.Z * oneMinusCos + a.Y * sinTheta,
                0,
                a.Y * a.X * oneMinusCos + a.Z * sinTheta,
                cosTheta + a.Y * a.Y * oneMinusCos,
                a.Y * a.Z * oneMinusCos - a.X * sinTheta,
                0,
                a.Z * a.X * oneMinusCos - a.Z * sinTheta,
                a.Z * a.Y * oneMinusCos + a.Z * sinTheta,
                cosTheta + a.Z * a.Z * oneMinusCos,
                0,
                0,
                0,
                0,
                1);
            return new Transform(m, m.Transpose());
        }

        public static Transform LookAt(Point from, Point look, Vector up)
        {
            var forward = (look - from).Normalize();
            var left = Vector.Cross(up.Normalize(), forward);
            var trueUp = Vector.Cross(forward, left);
            var m = new Matrix(
                left.X, trueUp.X, forward.X, from.X,
                left.Y, trueUp.Y, forward.Y, from.Y,
                left.Z, trueUp.Z, forward.Z, from.Z,
                0, 0, 0, 1
            );
            return new Transform(m.Inverse(), m);
        }
    }

    public static class TransformExtensions
    {
        public static Transform Translate(this Transform m, float x, float y, float z) =>
            Transform.Translate(x, y, z) * m;

        public static Transform Apply(this Transform m, in Transform n) => n * m;

        public static Transform TranslateX(this Transform m, float x) => Transform.TranslateX(x) * m;

        public static Transform TranslateY(this Transform m, float y) => Transform.TranslateY(y) * m;

        public static Transform TranslateZ(this Transform m, float z) => Transform.TranslateX(z) * m;

        public static Transform Scale(this Transform m, float x, float y, float z) => Transform.Scale(x, y, z) * m;

        public static Transform Scale(this Transform m, float factor) => Transform.Scale(factor) * m;

        public static Transform RotateX(this Transform m, float rad) => Transform.RotateX(rad) * m;

        public static Transform RotateY(this Transform m, float rad) => Transform.RotateY(rad) * m;

        public static Transform RotateZ(this Transform m, float rad) => Transform.RotateZ(rad) * m;

        public static Transform Shear(this Transform m, float xy, float xz, float yx, float yz, float zx, float zy) =>
            Transform.Shear(xy, xz, yx, yz, zx, zy) * m;
    }
}