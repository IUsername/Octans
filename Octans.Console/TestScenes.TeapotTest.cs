using System;
using System.Diagnostics;
using System.IO;
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
        public static void TeapotTest()
        {
            var path = Path.Combine(GetExecutionPath(), "teapot.obj");

            Console.WriteLine("Loading file {0}...", path);

            var data = ObjFile.ParseFile(path);
            Console.WriteLine("File parsed...");

            var triangulated = data.Groups[0];
            triangulated.SetTransform(Transforms.Scale(0.10f).RotateX(-MathF.PI / 2f).RotateY(MathF.PI / 8f));

            var glass = new Material
            {
                Texture = new SolidColor(new Color(0.2f, 0.6f, 0.3f)),
                Reflective = 0.78f,
                Roughness = 0.03f,
                Metallic = 0f,
                SpecularColor = new Color(0.3f,1f,0.5f),
                RefractiveIndex = 0.89f,
                Transparency = 0.83f,
                Ambient = 0.0f,
                Diffuse = 0.2f,
                Shininess = 200f,
                Specular = 0.9f
            };

            ApplyMaterialToChildren(triangulated, glass);

            var checkerboard = new Material
            {
                Texture = new CheckerTexture(new Color(1f, 1f, 0.5f), new Color(0.55f, 0.55f, 1f)),
                Reflective = 0.5f,
                Roughness = 0.2f,
                Ambient = 0.0f,
                Diffuse = 0.3f
            };

            checkerboard.Texture.SetTransform(Transforms.Scale(0.125f));

            var group = new Group();
            var floor = new Cube();
            floor.SetMaterial(checkerboard);
            floor.SetTransform(Transforms.TranslateY(-1).Scale(5f));
            group.AddChild(floor);
            group.AddChild(triangulated);
            group.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), new Color(1.8f, 1.8f, 1.8f)));
            w.SetObjects(group);

            //var width = 600;
            //var height = 400;
            //var transform = Transforms.View(new Point(0, 3.0f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0));
            //var c = new PinholeCamera(transform, MathF.PI / 3f, width, height);
            ////var ws = new ComposableWorldShading(3, GGXNormalDistribution.Instance, SchlickBeckmanGeometricShadow.Instance, SchlickFresnelFunction.Instance, w);
            //var ws = new PhongWorldShading(5, w);
            //var scene = new Scene(c, ws);
            //var aaa = new AdaptiveRenderer(3, 0.05f, scene);
            //var canvas = new Canvas(width, height);

            var width = 600;
            var height = 400;
            var from = new Point(0, 3.0f, -5f);
            var to = new Point(0, 1, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(3);
            var fov = MathF.PI / 3f;
            var aspectRatio = (float)width / height;
            var transform = Transforms.View(from, to, new Vector(0, 1, 0));
            var camera = new PinholeCamera(transform, fov, aspectRatio);
            var cws = new PhongWorldShading(5, w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //RenderContext.Render(canvas, aaa);
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void LowPolyTeapotTest()
        {
            var path = Path.Combine(GetExecutionPath(), "teapot-low.obj");
            var data = ObjFile.ParseFile(path);
            var triangulated = data.Groups[0];
            triangulated.SetTransform(Transforms.Scale(0.1f).RotateX(-MathF.PI / 2f));

            var material = new Material
            {
                Texture = new SolidColor(new Color(0.3f, 0.3f, 1f)),
                Reflective = 0.4f,
                Ambient = 0.2f,
                Diffuse = 0.3f
            };

            var floor = new Cube();
            floor.SetMaterial(material);
            var fg = new Group();
            fg.AddChild(floor);
            fg.SetTransform(Transforms.TranslateY(-1).Scale(1f));

            var g = new Group(fg, triangulated);
            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(g);

            //var width = 300;
            //var height = 200;
            //var transform = Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0));
            //var c = new PinholeCamera(transform, MathF.PI / 3f, width, height);
            //var scene = new Scene(c, new PhongWorldShading(1, w));
            //var canvas = new Canvas(width, height);

            var width = 300;
            var height = 200;
            var from = new Point(0, 1.5f, -5f);
            var to = new Point(0, 1, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(3);
            var fov = MathF.PI / 3f;
            var aspectRatio = (float)width / height;
            var transform = Transforms.View(from, to, new Vector(0, 1, 0));
            var camera = new PinholeCamera(transform, fov, aspectRatio);
            var cws = new PhongWorldShading(1, w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //RenderContext.Render(canvas, scene);
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}