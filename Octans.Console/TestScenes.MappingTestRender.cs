using System;
using System.Diagnostics;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {
        public static void MappingTestRender()
        {
            Cube Cube(float rotY, float rotX, float tx, float ty)
            {
                var cube1 = new Cube
                {
                    Material = {Pattern = CreateTestCubeMap()}
                };
                cube1.SetTransform(Transforms.RotateY(rotY).RotateX(rotX).Translate(tx, ty, 0));
                return cube1;
            }

            var g = new Group();
            g.AddChild(Cube(0.7854f, 0.7854f, -6, 2));
            g.AddChild(Cube(2.3562f, 0.7854f, -2, 2));
            g.AddChild(Cube(3.927f, 0.7854f, 2, 2));
            g.AddChild(Cube(5.4978f, 0.7854f, 6, 2));
            g.AddChild(Cube(0.7854f, -0.7854f, -6, -2));
            g.AddChild(Cube(2.3562f, -0.7854f, -2, -2));
            g.AddChild(Cube(3.927f, -0.7854f, 2, -2));
            g.AddChild(Cube(5.4978f, -0.7854f, 6, -2));

            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(0, 2, -100), Colors.White));
            w.SetObjects(g);

            var x = 800;
            var y = 400;
            var c = new Camera(x, y, 0.8f);
            c.SetTransform(Transforms.View(new Point(0, 0, -20f), new Point(0, 0, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w,0);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "mapping");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}