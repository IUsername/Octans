using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {
        public static void TeapotTest()
        {
            //path = Path.Combine(projectPath, "teapot-low.obj");
            var path = Path.Combine(GetExecutionPath(), "teapot.obj");


            Console.WriteLine("Loading file {0}...", path);

            var data = ObjFile.ParseFile(path);
            Console.WriteLine("File parsed...");

            var triangulated = data.Groups[0];
            triangulated.SetTransform(Transforms.Scale(0.10f).RotateX(-MathF.PI / 2f).RotateY(MathF.PI / 8f));

            var chrome = new Material
            {
                Pattern = new SolidColor(new Color(0.8f, 0.8f, 0.9f)),
                Reflective = 0.95f,
                RefractiveIndex = 0.86f,
                Transparency = 0.93f,
                Ambient = 0.02f,
                Diffuse = 0.2f,
                Shininess = 200f,
                Specular = 0.9f
            };

            ApplyMaterialToChildren(triangulated, chrome);


            var checkerboard = new Material
            {
                Pattern = new CheckerPattern(new Color(1f, 1f, 0.5f), new Color(0.55f, 0.55f, 1f)),
                Reflective = 0.2f,
                Ambient = 0.1f,
                Diffuse = 0.3f
            };

            checkerboard.Pattern.SetTransform(Transforms.Scale(0.125f));

            var group = new Group();
            var floor = new Cube();
            floor.SetMaterial(checkerboard);
            floor.SetTransform(Transforms.TranslateY(-1).Scale(5f));
            group.AddChild(floor);
            group.AddChild(triangulated);
            group.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), new Color(1.4f, 1.4f, 1.4f)));
            w.SetObjects(group);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 3.0f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.RenderAAA2(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void LowPolyTeapotTest()
        {
            var path = Path.Combine(GetExecutionPath(), "teapot-low.obj");
            var data = ObjFile.ParseFile(path);
            var tris = data.Groups[0];
            tris.SetTransform(Transforms.Scale(0.1f).RotateX(-MathF.PI / 2f));

            var material = new Material
            {
                Pattern = new SolidColor(new Color(0.3f, 0.3f, 1f)),
                Reflective = 0.4f,
                Ambient = 0.2f,
                Diffuse = 0.3f
            };

            var floor = new Cube();
            floor.SetMaterial(material);
            var fg = new Group();
            fg.AddChild(floor);
            fg.SetTransform(Transforms.TranslateY(-1).Scale(1f));

            var g = new Group(fg, tris);
            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(g);

            var x = 300;
            var y = 200;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}