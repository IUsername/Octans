using System;

namespace Octans
{
    public static class Transforms
    {
        public static Matrix Translate(double x, double y, double z) =>
            new Matrix(new[] {1.0, 0, 0, x},
                       new[] {0.0, 1, 0, y},
                       new[] {0.0, 0, 1, z},
                       new[] {0.0, 0, 0, 1});

        public static Matrix Scale(double x, double y, double z) =>
            new Matrix(new[] {x, 0, 0, 0},
                       new[] {0.0, y, 0, 0},
                       new[] {0.0, 0, z, 0},
                       new[] {0.0, 0, 0, 1});

        public static Matrix RotateX(double rad) =>
            new Matrix(new[] { 1.0, 0, 0, 0 },
                       new[] { 0.0, Math.Cos(rad), -Math.Sin(rad), 0 },
                       new[] { 0.0, Math.Sin(rad), Math.Cos(rad), 0 },
                       new[] { 0.0, 0, 0, 1 });

        public static Matrix RotateY(double rad) =>
            new Matrix(new[] { Math.Cos(rad), 0, Math.Sin(rad), 0 },
                       new[] { 0.0, 1, 0, 0 },
                       new[] { -Math.Sin(rad), 0, Math.Cos(rad), 0 },
                       new[] { 0.0, 0, 0, 1 });

        public static Matrix RotateZ(double rad) =>
            new Matrix(new[] { Math.Cos(rad), -Math.Sin(rad), 0,  0 },
                       new[] { Math.Sin(rad), Math.Cos(rad), 0, 0 },
                       new[] { 0.0, 0, 1, 0 },
                       new[] { 0.0, 0, 0, 1 });

        public static Matrix Shear(double xy, double xz, double yx, double yz, double zx, double zy) =>
            new Matrix(new[] { 1.0, xy, xz, 0 },
                       new[] { yx, 1, yz, 0 },
                       new[] { zx, zy, 1, 0 },
                       new[] { 0.0, 0, 0, 1 });

        public static Matrix Translate(this Matrix m, double x, double y, double z)
        {
            return Translate(x, y, z) * m;
        }

        public static Matrix Scale(this Matrix m, double x, double y, double z)
        {
            return Scale(x, y, z) * m;
        }

        public static Matrix RotateX(this Matrix m, double rad)
        {
            return RotateX(rad) * m;
        }

        public static Matrix RotateY(this Matrix m, double rad)
        {
            return RotateY(rad) * m;
        }

        public static Matrix RotateZ(this Matrix m, double rad)
        {
            return RotateZ(rad) * m;
        }

        public static Matrix Shear(this Matrix m, double xy, double xz, double yx, double yz, double zx, double zy)
        {
            return Shear(xy,xz,yx,yz,zx,zy) * m;
        }
    }
}