﻿using static System.MathF;
using static Octans.MathF;

namespace Octans
{
    public partial class Transform
    {
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
                0, Cos(rad), -Sin(rad), 0,
                0, Sin(rad), Cos(rad), 0,
                0, 0, 0, 1);

            return new Transform(m, m.Transpose());
        }

        public static Transform RotateY(float rad)
        {
            var m = new Matrix(
                Cos(rad), 0, Sin(rad), 0,
                0, 1, 0, 0,
                -Sin(rad), 0, Cos(rad), 0,
                0, 0, 0, 1);

            return new Transform(m, m.Transpose());
        }

        public static Transform RotateZ(float rad)
        {
            var m = new Matrix(
                Cos(rad), -Sin(rad), 0, 0,
                Sin(rad), Cos(rad), 0, 0,
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
            var sinTheta = Sin(theta);
            var cosTheta = Cos(theta);
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
            //var forward = (look - from).Normalize();
            //var left = Vector.Cross(forward, up.Normalize()).Normalize();
            //var trueUp = Vector.Cross(left, forward);
            //var orientation = new Matrix(new[] { left.X, left.Y, left.Z, 0 },
            //                             new[] { trueUp.X, trueUp.Y, trueUp.Z, 0 },
            //                             new[] { -forward.X, -forward.Y, -forward.Z, 0 },
            //                             new[] { 0.0f, 0, 0, 1 });
            //return new Transform(orientation * Transforms.Translate(-from.X, -from.Y, -from.Z));


            var forward = (from - look).Normalize();
            var left = Vector.Cross(up.Normalize(), forward).Normalize();
            var trueUp = Vector.Cross(forward, left);
            var m = new Matrix(
                left.X, trueUp.X, forward.X, from.X,
                left.Y, trueUp.Y, forward.Y, from.Y,
                left.Z, trueUp.Z, forward.Z, from.Z,
                0, 0, 0, 1
            );
            return new Transform(m.Inverse(), m);
        }

        public static Transform LookAt2(Point pos, Point look, Vector up)
        {
            //var forward = (look - from).Normalize();
            //var left = Vector.Cross(forward, up.Normalize()).Normalize();
            //var trueUp = Vector.Cross(left, forward);
            //var orientation = new Matrix(new[] { left.X, left.Y, left.Z, 0 },
            //                             new[] { trueUp.X, trueUp.Y, trueUp.Z, 0 },
            //                             new[] { -forward.X, -forward.Y, -forward.Z, 0 },
            //                             new[] { 0.0f, 0, 0, 1 });
            //return new Transform(orientation * Transforms.Translate(-from.X, -from.Y, -from.Z));


            var dir = (look - pos).Normalize();
            var right = Vector.Cross(up.Normalize(), dir).Normalize();
            var newUp = Vector.Cross(dir, right);
            var cameraToWorld = new Matrix(
                right.X, newUp.X, dir.X, pos.X,
                right.Y, newUp.Y, dir.Y, pos.Y,
                right.Z, newUp.Z, dir.Z, pos.Z,
                0, 0, 0, 1
            );
            return new Transform(cameraToWorld.Inverse(), cameraToWorld);
        }

        public static Transform Perspective(in float fov, float n, float f)
        {
            var persp = new Matrix(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, f / (f - n), -f * n / (f - n),
                0, 0, 1, 0);

            var invTanAng = 1f / Tan(Rad(fov) / 2f);
            return Scale(invTanAng, invTanAng, 1f) * new Transform(persp);
        }

        public static Transform Orthographic(float zNear, float zFar)
        {
            return Scale(1, 1, 1 / (zFar - zNear)) * Translate(0, 0, -zNear);
        }
    }
}