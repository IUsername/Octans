using System;
using System.Diagnostics;
using Octans.Geometry;
using Octans.Shading;

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

            var width = 800;
            var height = 400;
            var transform = Transforms.View(new Point(0, 0, -20f), new Point(0, 0, 0), new Vector(0, 1, 0));
            var c = new PinholeCamera(transform, 0.8f, width, height);
            var scene = new Scene(c, new PhongWorldShading(1, w));
            var aaa = new AdaptiveRenderer(3, 0.1f, scene);
            var canvas = new Canvas(width, height);

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            RenderContext.Render(canvas, aaa);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "mapping");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}