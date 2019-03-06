using System;

namespace Octans
{
    public static class Transforms
    {
        public static Matrix Translate(float x, float y, float z) =>
            new Matrix(new[] {1.0f, 0, 0, x},
                       new[] {0.0f, 1, 0, y},
                       new[] {0.0f, 0, 1, z},
                       new[] {0.0f, 0, 0, 1});

        public static Matrix Scale(float x, float y, float z) =>
            new Matrix(new[] {x, 0, 0, 0},
                       new[] {0.0f, y, 0, 0},
                       new[] {0.0f, 0, z, 0},
                       new[] {0.0f, 0, 0, 1});

        public static Matrix RotateX(float rad) =>
            new Matrix(new[] { 1.0f, 0, 0, 0 },
                       new[] { 0.0f, MathF.Cos(rad), -MathF.Sin(rad), 0 },
                       new[] { 0.0f, MathF.Sin(rad), MathF.Cos(rad), 0 },
                       new[] { 0.0f, 0, 0, 1 });

        public static Matrix RotateY(float rad) =>
            new Matrix(new[] { MathF.Cos(rad), 0, MathF.Sin(rad), 0 },
                       new[] { 0.0f, 1, 0, 0 },
                       new[] { -MathF.Sin(rad), 0, MathF.Cos(rad), 0 },
                       new[] { 0.0f, 0, 0, 1 });

        public static Matrix RotateZ(float rad) =>
            new Matrix(new[] { MathF.Cos(rad), -MathF.Sin(rad), 0,  0 },
                       new[] { MathF.Sin(rad), MathF.Cos(rad), 0, 0 },
                       new[] { 0.0f, 0, 1, 0 },
                       new[] { 0.0f, 0, 0, 1 });

        public static Matrix Shear(float xy, float xz, float yx, float yz, float zx, float zy) =>
            new Matrix(new[] { 1.0f, xy, xz, 0 },
                       new[] { yx, 1, yz, 0 },
                       new[] { zx, zy, 1, 0 },
                       new[] { 0.0f, 0, 0, 1 });

        public static Matrix Translate(this Matrix m, float x, float y, float z)
        {
            return Translate(x, y, z) * m;
        }

        public static Matrix Scale(this Matrix m, float x, float y, float z)
        {
            return Scale(x, y, z) * m;
        }

        public static Matrix RotateX(this Matrix m, float rad)
        {
            return RotateX(rad) * m;
        }

        public static Matrix RotateY(this Matrix m, float rad)
        {
            return RotateY(rad) * m;
        }

        public static Matrix RotateZ(this Matrix m, float rad)
        {
            return RotateZ(rad) * m;
        }

        public static Matrix Shear(this Matrix m, float xy, float xz, float yx, float yz, float zx, float zy)
        {
            return Shear(xy,xz,yx,yz,zx,zy) * m;
        }
    }
}