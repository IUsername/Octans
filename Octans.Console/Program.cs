using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Octans
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestRender();
            //TeapotTest();
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
                    Pattern = pattern,
                    Specular = 0.1f,
                    Reflective = 0.1f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1).Scale(20f));

            var middle = new Sphere {Material = {Pattern = perlin, Diffuse = 0.7f, Specular = 0.8f, Reflective = 0.4f, Shininess = 300}};
            middle.SetTransform(Transforms.Translate(-0.5f, 1f, 0.1f));

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
                Material = {Pattern = new GradientPattern(new Color(1f, 0, 0), new Color(1f, 0.8f, 0f))}
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
                Material = {Reflective = 0.8f, Specular = 0.8f, Diffuse = 0.4f, Ambient = 0.1f, Shininess = 200}
            };
            cylinder.SetTransform(Transforms.Translate(-3f, 0f, 3.5f));

            var t = new Triangle(new Point(0, 0, 0), new Point(1, 0.5f, 0), new Point(0.5f, 1f, 1f))
            {
                Material = {Pattern = new GradientPattern(new Color(0f, 1, 0), new Color(0f, 0f, 1f))}
            };
            t.SetTransform(Transforms.Translate(1f, 2f, 1f));

            var yellowGlass = new Material
            {
                Pattern = new SolidColor(new Color(1f, 0.8f, 0f)),
                Reflective = 0.4f,
                RefractiveIndex = 0.95f,
                //Transparency = 0.95f,
                Shininess = 300,
                Specular = 0.9f,
                Ambient = 0.1f,
                Diffuse = 0.3f
            };
            var co = new Cylinder
            {
                Minimum = -0.01f,
                Maximum = 0.01f,
                IsClosed = true
            };
            co.SetTransform(Transforms.Scale(1.5f,1f,1.5f));
            co.SetMaterial(yellowGlass);

            var ci = new Cylinder
            {
                Minimum = -0.1f,
                Maximum = 0.1f,
                IsClosed = true
            };
            ci.SetTransform(Transforms.Scale(1.2f, 1f, 1.2f));
            ci.SetMaterial(yellowGlass);

            var s  = new Solid(SolidOp.Difference, co, ci);
            s.SetTransform(Transforms.RotateZ(-0.2f).RotateX(-0.1f).Translate(-0.5f, 1f, 0.1f));

            var gl = new Group();
            gl.AddChild(middle);
            gl.AddChild(left);
            gl.AddChild(cylinder);
            gl.AddChild(s);

            var gr = new Group();
            gr.AddChild(cube);
            gr.AddChild(cone);
            gr.AddChild(t);
            

            var gt = new Group();
            gt.AddChild(gl);
            gt.AddChild(gr);
            gt.SetTransform(Transforms.TranslateZ(-0.5f));

            var gf = new Group();
            gf.AddChild(floor);

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(gt, gf);

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


            //path = Path.Combine(projectPath, "teapot-low.obj");
            path = Path.Combine(projectPath, "teapot.obj");


            Console.WriteLine("Loading file {0}...", path);

            var data = ObjFile.ParseFile(path);
            Console.WriteLine("File parsed...");

            var g = data.Groups[0];
            g.SetTransform(Transforms.Scale(0.15f).RotateX(-MathF.PI / 2f));

            var chrome = new Material
            {
                Pattern = new SolidColor(new Color(0.3f, 0.3f, 0.9f)),
                Reflective = 0.65f,
                Ambient = 0.05f,
                Diffuse = 0.35f,
                Shininess = 300f
            };

            ApplyMaterialToChildren(g, chrome);

            var checkerboard = new Material
            {
                Pattern = new CheckerPattern(new Color(0.5f, 0.5f, 0.5f), new Color(0.7f, 0.7f, 0.7f)),
                Reflective = 0.1f,
                Ambient = 0.2f,
                Diffuse = 0.3f
            };

            checkerboard.Pattern.SetTransform(Transforms.Scale(0.025f));

            var floor = new Cube();
            floor.SetMaterial(checkerboard);
            var fg = new Group();
            fg.AddChild(floor);
            fg.SetTransform(Transforms.TranslateY(-1).Scale(5f));

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(fg, g);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 3.0f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
            Console.ReadKey();
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