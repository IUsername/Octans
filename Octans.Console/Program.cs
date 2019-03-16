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
            //TestRender();
            TeapotTest();
        }

        public static void TestRender()
        {
            var s1 = new StripePattern(Colors.White, Colors.Black);
            var s2 = new StripePattern(Colors.White, Colors.Black);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositePattern(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var stripe = new StripePattern(new Color(0.9f, 0, 0), new Color(0.0f, 0.0f, 0.9f));
            stripe.SetTransform(Transforms.Scale(0.25f, 0.25f, 0.25f).RotateY(MathF.PI / 4));
            var perlin = new PerlinRippleCompositePattern(stripe, 0.8f);
            perlin.SetTransform(Transforms.Scale(0.1f, 0.1f, 0.1f));

            var floor = new Cube
            {
                Material =
                {
                    Pattern = pattern, // new CheckerPattern(new Color(0.75f, 0.75f, 0.75f), Colors.White),
                    Specular = 0f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1).Scale(20f));

            var middle = new Sphere { Material = { Pattern = perlin, Diffuse = 0.7f, Specular = 0.3f, Reflective = 0.4f } };
            middle.SetTransform(Transforms.Translate(-0.5f, 1f, 0.1f));
            //middle.SetTransform(Transforms.Translate(-0.5f, 0.5f, 0.5f));

            var right = new Sphere
            {
                Material =
                {
                    Pattern = new SolidColor(new Color(0.5f, 1f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f,
                    Reflective = 0.2f
                }
            };
            right.SetTransform(Transforms.Translate(1.5f, 0.5f, -0.5f) * Transforms.Scale(0.5f, 0.5f, 0.5f));

            var left = new Sphere
            {
                Material =
                {
                    Pattern = new SolidColor(new Color(0.9f, 0.9f, 1f)), Diffuse = 0.05f, Specular = 0.9f,
                    Transparency = 0.9f, RefractiveIndex = 1.52f, Reflective = 1.4f, Ambient = 0.11f, Shininess = 300
                }
            };
            left.SetTransform(Transforms.Translate(-2.1f, 0.33f, 0.5f) * Transforms.Scale(0.33f, 0.33f, 0.33f));

            var cube = new Cube
            {
                Material = { Pattern = new GradientPattern(new Color(1f, 0, 0), new Color(1f, 0.8f, 0f)) }
            };
            cube.Material.Pattern.SetTransform(Transforms.TranslateX(-0.5f).Scale(2f).RotateZ(MathF.PI / 2f));
            cube.SetTransform(Transforms.RotateY(MathF.PI / 4f).Translate(2.5f, 1f, 3.6f).Scale(1f, 1f, 1f));

            var cone = new Cone
            {
                IsClosed = true,
                Minimum = -1f,
                Maximum = 0f,
                Material =
                {
                    Pattern = new SolidColor(new Color(0.5f, 1f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f,
                    Reflective = 0.2f
                }
            };

            cone.SetTransform(Transforms.Scale(0.6f, 2f, 0.6f).Translate(1.5f, 2.0f, 0));

            var cylinder = new Cylinder
            {
                Minimum = 0f,
                Maximum = 3f,
                IsClosed = true,
                Material = { Reflective = 0.8f, Specular = 0.8f, Diffuse = 0.4f, Ambient = 0.1f, Shininess = 200 }
            };
            cylinder.SetTransform(Transforms.Translate(-3f, 0f, 3.5f));

            var t = new Triangle(new Point(0, 0, 0), new Point(1, 0.5f, 0), new Point(0.5f, 1f, 1f))
            {
                Material = { Pattern = new GradientPattern(new Color(0f, 1, 0), new Color(0f, 0f, 1f)) }
            };
            t.SetTransform(Transforms.Translate(1f, 2f, 1f));


            var gl = new Group();
            gl.AddChild(middle);
            gl.AddChild(left);
            gl.AddChild(cylinder);

            var gr = new Group();
            gr.AddChild(cube);
            gr.AddChild(cone);

            var g = new Group();
            g.AddChild(gl);
            g.AddChild(gr);
            g.SetTransform(Transforms.TranslateZ(-0.5f));

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(floor, g, t);
            //w.SetObjects(floor, cylinder, cube, middle, right, left);
            //w.SetObjects(floor, cube, middle, right, left);

            var x = 1200;
            var y = 800;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
            Console.ReadKey();
        }

        private static void TeapotTest()
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