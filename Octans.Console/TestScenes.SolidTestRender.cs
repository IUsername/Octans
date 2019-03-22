using System;
using System.Diagnostics;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {
        public static void SolidTestRender()
        {
            var radius = 0.2f;
            var red = new Material {Pattern = new SolidColor(new Color(1, 0, 0)), Reflective = 0.3f};
            var blue = new Material {Pattern = new SolidColor(new Color(0, 0, 1)), Reflective = 0.3f};
            var yellow = new Material {Pattern = new SolidColor(new Color(1, 1, 0)), Reflective = 0.3f};
            var white = new Material {Pattern = new SolidColor(new Color(1, 1, 1)), Reflective = 0.3f};
            var blackPip = new Material {Pattern = new SolidColor(new Color(0.1f, 0.1f, 0.1f))};
            var whitePip = new Material {CastsShadows = true};

            var d1 = CutPips(RoundedCube(radius, blue), whitePip);
            var d2 = CutPips(RoundedCube(radius, red), whitePip);
            var d3 = CutPips(RoundedCube(radius, white), blackPip);
            var d4 = CutPips(RoundedCube(radius, yellow), blackPip);

            d1.SetTransform(Transforms.RotateY(-2.2f).TranslateY(1f).Scale(0.5f));
            d2.SetTransform(Transforms.RotateZ(MathF.PI / 2f).TranslateY(1f).TranslateX(2f).TranslateZ(1f).Scale(0.5f));
            d3.SetTransform(Transforms.RotateY(0.5f).TranslateY(1f).TranslateX(-4f).TranslateZ(1f).Scale(0.5f));
            d4.SetTransform(Transforms.RotateY(-0.2f).TranslateY(3f).TranslateX(0.2f).TranslateZ(1.25f).Scale(0.5f));

            var lightGray = new Color(0.6f, 0.6f, 0.6f);
            var darkGray = new Color(0.4f, 0.4f, 0.4f);
            var s1 = new StripePattern(lightGray, darkGray);
            var s2 = new StripePattern(lightGray, darkGray);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositePattern(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var floor = new Cube
            {
                Material =
                {
                    Pattern = pattern,
                    Specular = 0.1f,
                    Diffuse = 0.3f,
                    Reflective = 0.15f,
                    Ambient = 0f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1f).Scale(20f));
            var g = new Group(d1, d2, d3, d4, floor);
            g.Divide(1);

            var w = new World();
            //w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetLights(new AreaLight(new Point(-3, 6, -4), new Vector(2f, 0, 0), 3, new Vector(0, 2f, 0), 3,
                                      new Color(1.4f, 1.4f, 1.4f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));
            //w.SetLights(new AreaLight(new Point(-10, 10, -10), new Vector(1,0,0), 4, new Vector(0,1,0), 3, Colors.White));
            w.SetObjects(g);

            var x = 600;
            var y = 400;
            var c = new Camera(x, y, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            Console.WriteLine("Rendering at {0}x{1}...", x, y);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var canvas = c.RenderAAA2(w, 1);
            //var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "solid");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}