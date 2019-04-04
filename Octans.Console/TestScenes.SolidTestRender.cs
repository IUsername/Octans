using System;
using System.Diagnostics;
using Octans.Camera;
using Octans.Geometry;
using Octans.IO;
using Octans.Light;
using Octans.Pipeline;
using Octans.Shading;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    internal static partial class TestScenes
    {
        public static void SolidTestRender()
        {
            var radius = 0.25f;
            var red = new Material {Texture = new SolidColor(new Color(1, 0, 0)), Reflective = 0.3f, Roughness = 0.3f, Ambient = 0.0f, SpecularColor = new Color(0.5f,0.5f,0.5f)};
            var blue = new Material {Texture = new SolidColor(new Color(0, 0, 1)), Reflective = 0.3f, Roughness = 0.3f, Ambient = 0.0f, SpecularColor = new Color(0.5f, 0.5f, 0.5f) };
            var yellow = new Material {Texture = new SolidColor(new Color(1, 1, 0)), Reflective = 0.3f, Roughness = 0.3f, Ambient = 0.0f, SpecularColor = new Color(0.5f, 0.5f, 0.5f) };
            var white = new Material {Texture = new SolidColor(new Color(1, 1, 1)), Reflective = 0.3f, Roughness = 0.3f, Ambient = 0.0f, SpecularColor = new Color(0.5f, 0.5f, 0.5f) };
            var blackPip = new Material {Texture = new SolidColor(new Color(0.1f, 0.1f, 0.1f))};
            var whitePip = new Material {CastsShadows = true};

            var d1 = CutPips(RoundedCube(radius, blue), whitePip);
            var d2 = CutPips(RoundedCube(radius, red), whitePip);
            var d3 = CutPips(RoundedCube(radius, white), blackPip);
            var d4 = CutPips(RoundedCube(radius, yellow), blackPip);

            d1.SetTransform(Transforms.RotateY(-2.2f).TranslateY(1f).Scale(0.5f));
            d2.SetTransform(Transforms.RotateZ(MathF.PI / 2f).TranslateY(1f).TranslateX(2f).TranslateZ(1f).Scale(0.5f));
            d3.SetTransform(Transforms.RotateY(0.5f).TranslateY(1f).TranslateX(-4f).TranslateZ(1f).Scale(0.5f));
            d4.SetTransform(Transforms.RotateY(-0.2f).TranslateY(3f).TranslateX(0.2f).TranslateZ(1.25f).Scale(0.5f));

            var lightGray = new Color(0.22f, 0.22f, 0.22f);
            var darkGray = new Color(0.15f, 0.15f, 0.15f);
            var s1 = new StripeTexture(lightGray, darkGray);
            var s2 = new StripeTexture(lightGray, darkGray);
            s2.SetTransform(Transforms.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositeTexture(s1, s2);
            pattern.SetTransform(Transforms.Scale(1f / 20f));

            var floor = new Cube
            {
                Material =
                {
                    Texture = pattern,
                    Roughness = 0.3f,
                    Specular = 0.1f,
                    Diffuse = 0.3f,
                    Reflective = 0.15f,
                    SpecularColor = new Color(0.0f, 0.0f,0.0f),
                    Ambient = 0f
                }
            };
            floor.SetTransform(Transforms.TranslateY(-1f).Scale(20f));
            var g = new Group(d1, d2, d3, d4, floor);
            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(-8, 10, -10), new Color(0.9f, 0.9f, 0.9f)));
            //w.SetLights(new AreaLight(new Point(-3, 6, -4), new Vector(1f, 0, 0), 3, new Vector(0, 1f, 0), 3,
            //                          new Color(1.4f, 1.4f, 1.4f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));
            //w.SetLights(new AreaLight(new Point(-10, 10, -10), new Vector(1,0,0), 4, new Vector(0,1,0), 3, Colors.White));
            w.SetObjects(g);

            var width = 600;
            var height = 400;
            var transform = Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0));
            var c = new PinholeCamera(transform, MathF.PI / 3f, width, height);
            var ws = new ComposableWorldShading(2, GGXNormalDistribution.Instance, SchlickBeckmanGeometricShadow.Instance, SchlickFresnelFunction.Instance, w);
            //var ws = new PhongWorldShading(2, w);
            var scene = new Scene(c, ws);
            //var aaa = new AdaptiveRenderer(2, 0.01f, scene);
            var aaa = new SamplesPerPixelRenderer(24, scene);
            var canvas = new Canvas(width, height);

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            RenderContext.Render(canvas, aaa);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "solid");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}