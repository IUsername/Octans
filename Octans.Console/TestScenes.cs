using System;
using System.IO;
using System.Linq;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {

        private static string GetExecutionPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(d => string.Equals("bin", d));
            return string.Join(Path.DirectorySeparatorChar.ToString(),
                               pathItems.Take(pathItems.Length - pos - 1));
        }

        private static Solid RoundedCube(float radius, Material mat)
        {
            Solid SolidFaces(float r)
            {
                var cY = new Cube();
                cY.SetTransform(Transforms.Scale(1f - r, 1f, 1f - r));
                cY.SetMaterial(mat);

                var cX = new Cube();
                cX.SetTransform(Transforms.Scale(1f, 1f - r, 1f - r));
                cX.SetMaterial(mat);
                var su = new Solid(SolidOp.Union, cY, cX);

                var cZ = new Cube();
                cZ.SetTransform(Transforms.Scale(1f - r, 1f - r, 1f));
                cZ.SetMaterial(mat);
                return new Solid(SolidOp.Union, su, cZ);
            }

            Solid Union(IShape a, IShape b) => new Solid(SolidOp.Union, a, b);

            Cylinder CreateCylinder(float r, Point from, Material material, Matrix rotation)
            {
                var dist = 1f - r;
                var fOffset = from * dist;
                var e = new Cylinder {Minimum = 0f, Maximum = 2f * dist / r, IsClosed = true};
                e.SetMaterial(material);
                e.SetTransform(Transforms.Scale(r).Apply(rotation).Translate(fOffset.X, fOffset.Y, fOffset.Z));
                return e;
            }

            Sphere Corner(float r, Point corner, Material material)
            {
                var dist = 1f - r;
                var offset = corner * dist;
                var sphere = new Sphere();
                sphere.SetTransform(Transforms.Scale(r).Translate(offset.X, offset.Y, offset.Z));
                sphere.SetMaterial(material);
                return sphere;
            }

            Cylinder EdgeX(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Transforms.RotateZ(-MathF.PI / 2));

            Cylinder EdgeY(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Matrix.Identity);

            Cylinder EdgeZ(float r, Point from, Material material) =>
                CreateCylinder(r, from, material, Transforms.RotateX(MathF.PI / 2));

            var s = SolidFaces(radius);
            var points = new[]
            {
                new Point(-1, 1, -1),
                new Point(1, 1, -1),
                new Point(1, 1, 1),
                new Point(-1, 1, 1),
                new Point(-1, -1, -1),
                new Point(1, -1, -1),
                new Point(1, -1, 1),
                new Point(-1, -1, 1)
            };

            foreach (var point in points)
            {
                s = Union(s, Corner(radius, point, mat));
                if (point.X < 0f)
                {
                    s = Union(s, EdgeX(radius, point, mat));
                }

                if (point.Y < 0f)
                {
                    s = Union(s, EdgeY(radius, point, mat));
                }

                if (point.Z < 0f)
                {
                    s = Union(s, EdgeZ(radius, point, mat));
                }
            }

            return s;
        }

        private static Solid CutPips(Solid solid, Material material)
        {
            Sphere PipSphere(Point point, Material mat)
            {
                var sphere = new Sphere();
                sphere.SetTransform(Transforms.Scale(0.2f).Translate(point.X, point.Y, point.Z));
                sphere.SetMaterial(mat);
                return sphere;
            }

            Solid Diff(IShape s, IShape child) => new Solid(SolidOp.Difference, s, child);

            var offset = 1.15f;

            // 1
            solid = Diff(solid, PipSphere(new Point(0, 0, -offset), material));

            //2
            solid = Diff(solid, PipSphere(new Point(0.4f, offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, offset, -0.4f), material));

            //3
            solid = Diff(solid, PipSphere(new Point(-offset, 0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-offset, -0.4f, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-offset, 0.0f, 0.0f), material));

            //4        
            solid = Diff(solid, PipSphere(new Point(offset, 0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, -0.4f, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, -0.4f, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(offset, 0.4f, -0.4f), material));

            //5       
            solid = Diff(solid, PipSphere(new Point(0.4f, -offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -offset, -0.4f), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -offset, 0.4f), material));
            solid = Diff(solid, PipSphere(new Point(0.4f, -offset, -0.4f), material));

            //6       
            solid = Diff(solid, PipSphere(new Point(0.4f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.0f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, 0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.4f, -0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(0.0f, -0.4f, offset), material));
            solid = Diff(solid, PipSphere(new Point(-0.4f, -0.4f, offset), material));

            return solid;
        }

        private static CubeMap CreateTestCubeMap()
        {
            var orange = new Color(1f, 0.5f, 0f);
            var left = new UVAlignTestPattern(Colors.Yellow, Colors.Cyan, Colors.Red, Colors.Blue, orange);
            var front = new UVAlignTestPattern(Colors.Cyan, Colors.Red, Colors.Yellow, orange, Colors.Green);
            var right = new UVAlignTestPattern(Colors.Red, Colors.Yellow, Colors.Magenta, Colors.Green, Colors.White);
            var back = new UVAlignTestPattern(Colors.Green, Colors.Magenta, Colors.Cyan, Colors.White, Colors.Blue);
            var top = new UVAlignTestPattern(orange, Colors.Cyan, Colors.Magenta, Colors.Red, Colors.Yellow);
            var bottom = new UVAlignTestPattern(Colors.Magenta, orange, Colors.Green, Colors.Blue, Colors.White);
            return new CubeMap(left, front, right, back, top, bottom);
        }

        private static void ApplyMaterialToChildren(Group group, Material material)
        {
            foreach (var child in group.Children)
            {
                child.SetMaterial(material);
            }
        }
    }
}