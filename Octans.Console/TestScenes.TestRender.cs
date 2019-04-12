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
        public static void TestRender(int spp)
        {
            var w = BuildWorld();

            var width = 600;
            var height = 400;
            var canvas = new Canvas(width, height);

            var pps = new PerPixelSampler(spp);
            var camera = new ApertureCamera(MathF.PI / 3f, 3f / 2, 0.05f,
                                             new Point(0, 1.25f, -4f),
                                             new Point(0, 1, 0), Vectors.Up, 3.5f);
            var cws = new ComposableWorldSampler(2,
                                                 16,
                                                 GGXNormalDistribution.Instance,
                                                 GGXSmithGeometricShadow.Instance, 
                                                 SchlickFresnelFunction.Instance,
                                                 w);

            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        private static World BuildWorld()
        {
            Console.WriteLine("Loading file...");
            var filePath = Path.Combine(GetExecutionPath(), "indoor_env.ppm");
            //var filePath = Path.Combine(GetExecutionPath(), "winter_river_1k.ppm");
            Console.WriteLine("Parsing file...");
            var textureCanvas = PPM.ParseFile(filePath);
            var image = new UVImage(textureCanvas);
            var map = new TextureMap(image, UVMapping.Spherical);

            var skySphere = new Sphere
            {
                Material =
                {
                    Texture = map, Ambient = 1.5f, CastsShadows = false, Transparency = 0f, Roughness = 1f,
                    SpecularColor = new Color(0.0f, 0.0f, 0.0f)
                }
            };

            //skySphere.SetTransform(Transforms.RotateY(3.4f).Scale(1000f));
            skySphere.SetTransform(Transform.RotateY(3.3f).Scale(10000f));

            var s1 = new StripeTexture(Colors.White, Colors.Black);
            var s2 = new StripeTexture(Colors.White, Colors.Black);
            s2.SetTransform(Transform.RotateY(MathF.PI / 2));
            var pattern = new BlendedCompositeTexture(s1, s2);
            pattern.SetTransform(Transform.Scale(1f / 20f));

            var testPattern =
                new UVAlignTestPattern(Colors.White, Colors.Red, Colors.Yellow, Colors.Green, Colors.Blue);
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
                    Texture = pattern,
                    Specular = 0.1f,
                    Reflective = 0.1f,
                    Roughness = 0.22f,
                    Ambient = 0f,
                    SpecularColor = new Color(0.1f, 0.1f, 0.1f)
                }
            };
            floor.SetTransform(Transform.TranslateY(-1).Scale(20f));

            var middle = new Sphere
            {
                Material =
                {
                    Texture = worldPattern, Diffuse = 0.7f, Specular = 1f, Reflective = 0.4f, Shininess = 600,
                    Roughness = 0.2f, Metallic = 0.3f, SpecularColor = new Color(0.1f, 0.2f, 0.5f), Ambient = 0f
                }
            };
            middle.SetTransform(Transform.RotateY(1.5f).Translate(-0.5f, 1f, 0.1f));

            var right = new Sphere
            {
                Material =
                {
                    Texture = new TextureMap(new UVCheckers(20, 10, Colors.Black, Colors.White), UVMapping.Spherical),
                    Roughness = 0.5f,
                    Diffuse = 0.7f,
                    Specular = 0.3f,
                    Reflective = 0.2f,
                    Ambient = 0f,
                    SpecularColor = new Color(0.2f, 0.2f, 0.2f)
                }
            };
            right.SetTransform(Transform.Translate(0.25f, 0.25f, -1f) * Transform.Scale(0.25f));

            var rightPlastic = new Sphere
            {
                Material =
                {
                    Texture = new SolidColor(new Color(1f, 0.0f, 0.0f)),
                    SpecularColor = new Color(0.2f,0.2f,0.2f),
                    Roughness = 0.25f, Metallic = 0f,
                    Ambient = 0.0f
                }
            };
            rightPlastic.SetTransform(Transform.Translate(-0.15f, 0.15f, -0.91f) * Transform.Scale(0.15f));

            var left = new Sphere
            {
                Material =
                {
                    Texture = new SolidColor(new Color(0.85f, 0.85f, 0.90f)), 
                    SpecularColor = new Color(0.3f,0.3f,0.3f),
                    Roughness = 0.10f, Metallic = 0.6f,
                    Transparency = 0.95f, RefractiveIndex = 1.52f,  Ambient = 0.0f
                }
            };
            left.SetTransform(Transform.Translate(-1.3f, 0.30f, -0.75f) * Transform.Scale(0.30f));
           // left.SetTransform(Transforms.Translate(-2.1f, 0.33f, 0.5f) * Transforms.Scale(0.33f, 0.33f, 0.33f));

            var leftChrome = new Sphere
            {
                Material =
                {
                    Texture = new SolidColor(new Color(0.9f, 0.9f, 0.95f)), Diffuse = 0.05f, Specular = 0.9f,
                    SpecularColor = new Color(0.9f,0.9f,0.95f),
                    Roughness = 0.55f, Metallic = 1f,
                    Transparency = 0, RefractiveIndex = 1.52f, Reflective = 1.4f, Ambient = 0.0f, Shininess = 300
                }
            };
            leftChrome.SetTransform(Transform.Translate(-0.95f, 0.15f, -1.1f) * Transform.Scale(0.15f));

            var cube = new Cube
            {
                Material =
                {
                    Texture = new GradientTexture(new Color(1f, 0, 0), new Color(1f, 0.8f, 0f)), Roughness = 0.5f,
                    Ambient = 0f, SpecularColor = new Color(0.2f, 0.2f, 0.2f)
                }
            };
            cube.Material.Texture.SetTransform(Transform.TranslateX(-0.5f).Scale(2f).RotateZ(MathF.PI / 2f));
            cube.SetTransform(Transform.RotateY(MathF.PI / 4f).Translate(2.5f, 1f, 3.6f).Scale(1f, 1f, 1f));

            var cone = new Cone
            {
                IsClosed = true,
                Minimum = -1f,
                Maximum = 0f,
                Material =
                {
                    Texture = new SolidColor(new Color(0.4f, 0.8f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f,
                    Ambient = 0f,
                    SpecularColor = new Color(0.2f, 0.2f, 0.2f),
                    Roughness = 0.6f,
                    Reflective = 0.2f
                }
            };
            cone.SetTransform(Transform.Scale(0.6f, 2f, 0.6f).Translate(1.5f, 2.0f, 0));

            var cylinder = new Cylinder
            {
                Minimum = 0f,
                Maximum = 3f,
                IsClosed = true,
                Material =
                {
                    Reflective = 0.33f, Specular = 0.9f, Diffuse = 0.1f, Ambient = 0.0f, Shininess = 100,
                    Metallic = 0.76f, SpecularColor = new Color(0.9f, 0.8f, 0.7f), Roughness = 0.02f,
                    Texture = testMap
                }
            };
            cylinder.SetTransform(Transform.Translate(-3f, 0f, 3.5f));

            //var t = new Triangle(new Point(0, 0, 0), new Point(1, 0.5f, 0), new Point(0.5f, 1f, 1f))
            //{
            //    Material = {Texture = new GradientTexture(new Color(0f, 1, 0), new Color(0f, 0f, 1f))}
            //};
            //t.SetTransform(Transforms.Translate(1f, 2f, 1f));

            var ringMaterial = new Material
            {
                Texture = new SolidColor(new Color(1f, 0.8f, 0f)),
                Reflective = 0.4f,
                RefractiveIndex = 0.95f,
                Roughness = 0.12f,
                Metallic = 1f,
                SpecularColor = new Color(0.9f, 0.7f, 0f),
                //Transparency = 0.95f,
                Shininess = 300,
                Specular = 0.9f,
                Ambient = 0.0f,
                Diffuse = 0.3f
            };
            var co = new Cylinder
            {
                Minimum = -0.01f,
                Maximum = 0.01f,
                IsClosed = true
            };
            co.SetTransform(Transform.Scale(1.5f, 1f, 1.5f));
            co.SetMaterial(ringMaterial);

            var ci = new Cylinder
            {
                Minimum = -0.1f,
                Maximum = 0.1f,
                IsClosed = true
            };
            ci.SetTransform(Transform.Scale(1.2f, 1f, 1.2f));
            ci.SetMaterial(ringMaterial);

            var s = new ConstructiveSolid(ConstructiveOp.Difference, co, ci);
            s.SetTransform(Transform.RotateZ(-0.2f).RotateX(-0.1f).Translate(-0.5f, 1f, 0.1f));

            var gl = new Group();
            gl.AddChild(middle);
            gl.AddChild(left);
            gl.AddChild(leftChrome);
            gl.AddChild(cylinder);
            gl.AddChild(s);
            gl.AddChild(right);
            gl.AddChild(rightPlastic);
            gl.AddChild(cube);
            gl.AddChild(cone);
            //gl.AddChild(t);
            gl.AddChild(floor);
            gl.AddChild(skySphere);

            gl.Divide(1);

            var w = new World();
            w.SetLights(new AreaLight(new Point(-80f, 80, -60), new Vector(100f, 0, 0), 6, new Vector(0, 0f, -10f), 3,
                                      new Color(1.8f, 1.8f, 1.8f), new Sequence(0.7f, 0.3f, 0.9f, 0.1f, 0.5f)));

            //w.SetLights(new PointLight(new Point(-3.5f, 4f, -5f), new Color(0.9f, 0.9f, 0.9f)));
            w.SetObjects(gl);
            return w;
        }
    }
}