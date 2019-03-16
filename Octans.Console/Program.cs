using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Octans
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(d => string.Equals("bin", d));
            var projectPath = string.Join(Path.DirectorySeparatorChar.ToString(),
                                          pathItems.Take(pathItems.Length - pos - 1));


            path = Path.Combine(projectPath, "teapot-low.obj");


            Console.WriteLine("Loading file {0}...", path);

            var data = ObjFile.ParseFile(path);
            Console.WriteLine("File parsed...");

            var g = data.Groups[0];
            g.SetTransform(Transforms.Scale(0.1f).RotateX(-MathF.PI / 2f));

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

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(fg, g);

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
            Console.ReadKey();
        }
    }
}