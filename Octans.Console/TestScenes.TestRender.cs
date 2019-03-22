using System;
using System.Diagnostics;
using System.IO;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {
        public static void TestRender()
        {
            var s1 = new StripePattern(Colors.White, Colors.Black);
            var s2 = new StripePattern(Colors.White, Colors.Black);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositePattern(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var testPattern = new UVAlignTestPattern(Colors.White, Colors.Red, Colors.Yellow, Colors.Green, Colors.Blue);
            var testMap = new TextureMap(testPattern, UVMapping.Cylindrical);

            //var stripe = new StripePattern(new Color(0.9f, 0, 0), new Color(0.0f, 0.0f, 0.9f));
            //stripe.SetTransform(Transforms.Scale(0.25f, 0.25f, 0.25f).RotateY(MathF.PI / 4));
            //var perlin = new PerlinRippleCompositePattern(stripe, 0.8f);
            //perlin.SetTransform(Transforms.Scale(0.1f, 0.1f, 0.1f));

            var worldFilePath = Path.Combine(GetExecutionPath(), "world.ppm");
            var worldCanvas = PPM.ParseFile(worldFilePath);
            var worldTexture = new UVImage(worldCanvas);
            var worldPattern = new TextureMap(worldTexture, UVMapping.Spherical);

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

            var middle = new Sphere
                {Material = {Pattern = worldPattern, Diffuse = 0.7f, Specular = 1f, Reflective = 0.4f, Shininess = 600}};
            middle.SetTransform(Transforms.RotateY(1.5f).Translate(-0.5f, 1f, 0.1f));

            var right = new Sphere
            {
                Material =
                {
                    Pattern = new TextureMap(new UVCheckers(20, 10, Colors.Black, Colors.White), UVMapping.Spherical),
                    Diffuse = 0.7f, Specular = 0.3f,
                    Reflective = 0.2f
                }
            };
            right.SetTransform(Transforms.Translate(0.25f, 0.25f, -0.75f) * Transforms.Scale(0.25f));

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
                Material =
                {
                    Reflective = 0.33f, Specular = 0.9f, Diffuse = 0.1f, Ambient = 0.01f, Shininess = 100,
                    Pattern = testMap
                }
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
            co.SetTransform(Transforms.Scale(1.5f, 1f, 1.5f));
            co.SetMaterial(yellowGlass);

            var ci = new Cylinder
            {
                Minimum = -0.1f,
                Maximum = 0.1f,
                IsClosed = true
            };
            ci.SetTransform(Transforms.Scale(1.2f, 1f, 1.2f));
            ci.SetMaterial(yellowGlass);

            var s = new Solid(SolidOp.Difference, co, ci);
            s.SetTransform(Transforms.RotateZ(-0.2f).RotateX(-0.1f).Translate(-0.5f, 1f, 0.1f));

            var gl = new Group();
            gl.AddChild(middle);
            gl.AddChild(left);
            gl.AddChild(cylinder);
            gl.AddChild(s);
            gl.AddChild(right);
            gl.AddChild(cube);
            gl.AddChild(cone);
            gl.AddChild(t);
            gl.AddChild(floor);

            gl.Divide(1);

            var w = new World();
            //w.SetLights(new AreaLight(new Point(-3f, 4, -5), new Vector(1f, 0, 0), 6, new Vector(0, 0.01f, 0), 3,
            //                          new Color(1.4f, 1.4f, 1.4f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));
            w.SetLights(new PointLight(new Point(-3.5f,4f,-5f), new Color(1.4f, 1.4f, 1.4f)));
            w.SetObjects(gl);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.25f, -4f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.Render(w, 4, 0.03f);
            //var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}