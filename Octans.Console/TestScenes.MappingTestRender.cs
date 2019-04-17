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
        public static void MappingTestRender()
        {
            static Cube Cube(float rotY, float rotX, float tx, float ty)
            {
                var cube1 = new Cube
                {
                    Material = {Texture = CreateTestCubeMap(), Roughness = 1f, SpecularColor = new Color(0.3f,0.3f,0.3f)}
                };
                cube1.SetTransform(Transform.RotateY(rotY).RotateX(rotX).Translate(tx, ty, 0));
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
            var from = new Point(0, 0, -20f);
            var to = new Point(0, 0, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(16);
            var fov = 0.8f;
            var aspectRatio = (float) width/height;
            var transform = Transform.LookAt(from, to, new Vector(0, 1, 0));
            var camera = new PinholeCamera(transform, fov, aspectRatio);
            var cws = new PhongWorldShading(1, w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
          
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "mapping");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void SkyBoxMappingTestRender()
        {
            Console.WriteLine("Loading sky box file...");
            var skyBoxFile = Path.Combine(GetExecutionPath(), "skyboxsun.ppm");
            Console.WriteLine("Parsing sky box file...");
            var skyBoxCanvas = PPM.ParseFile(skyBoxFile);
            var skyBoxTexture = new UVImage(skyBoxCanvas);
            var skyBoxMap = new SkyBoxMap(skyBoxTexture);


            Cube Cube(float rotY, float rotX, float tx, float ty)
            {
                var cube1 = new Cube
                {
                    Material = { Texture = skyBoxMap }
                };
                cube1.SetTransform(Transform.RotateY(rotY).RotateX(rotX).Translate(tx, ty, 0));
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
            var from = new Point(0, 0, -20f);
            var to = new Point(0, 0, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(16);
            var fov = 0.8f;
            var aspectRatio = (float)width / height;
            var transform = Transform.LookAt(from, to, new Vector(0, 1, 0));
            var camera = new PinholeCamera(transform, fov, aspectRatio);
            var cws = new PhongWorldShading(1, w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "mapping_sky_box");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void SphereMappingTestRender()
        {
            Console.WriteLine("Loading file...");
            var filePath = Path.Combine(GetExecutionPath(), "winter_river_1k.ppm");
            Console.WriteLine("Parsing file...");
            var textureCanvas = PPM.ParseFile(filePath);
            var image = new UVImage(textureCanvas);
            var map = new TextureMap(image, UVMapping.Spherical);


            Sphere Sphere(float rotY, float rotX, float tx, float ty)
            {
                var s = new Sphere()
                {
                    Material = { Texture = map }
                };
                s.SetTransform(Transform.RotateY(rotY).RotateX(rotX).Translate(tx, ty, 0));
                return s;
            }

            var g = new Group();
            g.AddChild(Sphere(0.7854f, 0.7854f, -6, 2));
            g.AddChild(Sphere(2.3562f, 0.7854f, -2, 2));
            g.AddChild(Sphere(3.927f, 0.7854f, 2, 2));
            g.AddChild(Sphere(5.4978f, 0.7854f, 6, 2));
            g.AddChild(Sphere(0.7854f, -0.7854f, -6, -2));
            g.AddChild(Sphere(2.3562f, -0.7854f, -2, -2));
            g.AddChild(Sphere(3.927f, -0.7854f, 2, -2));
            g.AddChild(Sphere(5.4978f, -0.7854f, 6, -2));

            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(0, 2, -100), Colors.White));
            w.SetObjects(g);

            var width = 800;
            var height = 400;
            var from = new Point(0, 0, -20f);
            var to = new Point(0, 0, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(16);
            var fov = 0.8f;
            var aspectRatio = (float)width / height;
            var transform = Transform.LookAt(from, to, new Vector(0, 1, 0));
            var camera = new PinholeCamera(transform, fov, aspectRatio);
            var cws = new PhongWorldShading(1, w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "mapping_spheres");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void InsideSphere()
        {
            Console.WriteLine("Loading file...");
            var filePath = Path.Combine(GetExecutionPath(), "winter_river_1k.ppm");
            Console.WriteLine("Parsing file...");
            var textureCanvas = PPM.ParseFile(filePath);
            var image = new UVImage(textureCanvas);
            var map = new TextureMap(image, UVMapping.Spherical);

            var skySphere = new Sphere()
            {
                Material = { Texture = map, Ambient = 1.2f, CastsShadows = false, Transparency = 1f}
            };

            skySphere.SetTransform(Transform.RotateY(2.1f).Scale(1000f));

            var s = new Sphere
            {
                Material =
                {
                    Roughness = 0.1f,
                    Texture = SolidColor.Create(new Color(1f, 0.0f, 0.0f)),
                    SpecularColor = new Color(0.2f, 0.2f, 0.2f),
                    Metallic = 0f,
                    Ambient = 0f
                }
            };

            var g = new Group();
            g.AddChild(skySphere);
            g.AddChild(s);

            g.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(100, 100, -100), new Color(1f,1f,1f)));
            w.SetObjects(g);

            var width = 400;
            var height = 400;
            var from = new Point(0, -0.8f, -4f);
            var to = new Point(0, 0, 0);

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(1000);
            var fov = 0.8f;
            var aspectRatio = (float)width / height;
            var camera = new ApertureCamera(fov, aspectRatio, 0.05F, from, to,Vectors.Up);
            var cws = new ComposableWorldSampler(1,
                                                 2,
                                                 GGXNormalDistribution.Instance,
                                                 SchlickBeckmanGeometricShadow.Instance,
                                                 SchlickFresnelFunction.Instance,
                                                 w);
            var ctx = new RenderContext(canvas, new RenderPipeline(cws, camera, pps));

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ctx.Render();
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "sky_sphere");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void RowMetal(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f, 0.3f, 0.3f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = color;
                material.Metallic = 1f;
                material.Ambient = 0f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "metal", i =>
            {
                var metal = CreateMaterial();
                metal.Roughness = MathF.Saturate((i - 1) * delta + 0.01f);
                return metal;
            });
        }

        public static void RowPlastic(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f, 0.3f, 0.3f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = new Color(0.2f, 0.2f, 0.2f);
                material.Metallic = 0f;
                material.Ambient = 0f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "plastic", i =>
            {
                var metal = CreateMaterial();
                metal.Roughness = MathF.Saturate((i - 1) * delta + 0.01f);
                return metal;
            });
        }

        public static void RowMetalPlastic(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f, 0.3f, 0.3f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = new Color(0.2f, 0.2f, 0.2f);
                material.Metallic = 0f;
                material.Ambient = 0f;
                material.Roughness = 0.1f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "plastic_metal", i =>
            {
                var metal = CreateMaterial();
                metal.Metallic = MathF.Saturate((i - 1) * delta);
                return metal;
            });
        }

        public static void RowTransparent(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f, 0.3f, 0.3f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = new Color(0.6f, 0.6f, 0.6f);
                material.Roughness = 0.1f;
                material.Metallic = 0.3f;
                material.RefractiveIndex = 1.51f;
                material.Ambient = 0f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "transparent", i =>
            {
                var metal = CreateMaterial();
                metal.Transparency = MathF.Saturate((i - 1) * delta + 0.01f);
                return metal;
            });
        }

        public static void RowTransparentRefraction(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f, 0.3f, 0.3f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = new Color(0.6f, 0.6f, 0.6f);
                material.Roughness = 0.1f;
                material.Metallic = 0.3f;
                material.RefractiveIndex = 1.51f;
                material.Transparency = 0.90f;
                material.Ambient = 0f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "transparent_ior", i =>
            {
                var value = MathF.Saturate((i - 1) * delta);
                var metal = CreateMaterial();
                metal.RefractiveIndex = MathF.Lerp(1f,2f, value);
                return metal;
            });
        }

        public static void RowTransparentRoughness(int spp)
        {
            static MaterialInfo CreateMaterial()
            {
                var material = new MaterialInfo();
                var color = new Color(1f,1f, 1f);
                material.Texture = SolidColor.Create(color);
                material.SpecularColor = new Color(0.8f, 0.8f, 0.8f);
                material.Transparency = 0.92f;
                material.Metallic = 0.65f;
                material.RefractiveIndex = 1.51f;
                material.Ambient = 0f;
                return material;
            }

            const float delta = 1f / 9;

            RowTestByDelegate(spp, "transparent_roughness", i =>
            {
                var metal = CreateMaterial();
                metal.Roughness = MathF.Saturate((i - 1) * delta);
                return metal;
            });
        }

        public static void ColRowTestRender()
        {
            Console.WriteLine("Loading file...");
            //var filePath = Path.Combine(GetExecutionPath(), "indoor_env.ppm");
            var filePath = Path.Combine(GetExecutionPath(), "winter_river_1k.ppm");
            Console.WriteLine("Parsing file...");
            var textureCanvas = PPM.ParseFile(filePath);
            var image = new UVImage(textureCanvas);
            var map = new TextureMap(image, UVMapping.Spherical);

            var skySphere = new Sphere()
            {
                Material = { Texture = map, Ambient = 1.5f, CastsShadows = false, Transparency = 1f }
            };

            skySphere.SetTransform(Transform.RotateY(3.4f).Scale(1000f));

            var g = new Group();
            var dx = 2.75f;
            var dz = 3.5f;
            var y = 1f;
            var nX = 10;
            var nZ = 1;
            var delta = 1f / (nX * nZ - 1);
            int n = 0;
            bool metallic = false;
            for (var z = 0; z < nZ; z++)
            {
                for (var x = 0; x < nX; x++)
                {
                    var s = new Sphere();
                    s.SetTransform(Transform.TranslateY(1f).Scale(1.2f).Translate(x * dx, 0, z * dz));
                   // var color = x % 2 == 0 ? new Color(1f, 1f, 1f) : new Color(1f, 0.3f, 0.3f);
                    var color = new Color(1f, 0.3f, 0.3f);
                    s.Material.Texture = SolidColor.Create(color);
                    s.Material.SpecularColor = metallic ? color : new Color(0.2f,0.2f,0.2f);
                    s.Material.Roughness =  MathF.Saturate(n * delta + 0.01f);
                    s.Material.Metallic = metallic ? 1f : 0f;
                    s.Material.Ambient = 0f;
                    s.Material.Reflective = 0.9f;
                    g.AddChild(s);
                    n++;
                }
            }

            var lightGray = new Color(0.9f, 0.9f, 0.9f);
            var darkGray = new Color(0.1f, 0.9f, 0.1f);
            var s1 = new StripeTexture(lightGray, darkGray);
            var s2 = new StripeTexture(lightGray, darkGray);
            s2.SetTransform(Transform.RotateY(System.MathF.PI / 2));
            var pattern = new BlendedCompositeTexture(s1, s2);
            pattern.SetTransform(Transform.Scale(1f / 30f));

            var text = new CheckerTexture(new Color(0.3f,0.7f,0.3f), new Color(0.13f, 0.13f, 0.13f));
            text.SetTransform(Transform.Scale(1f / 16f));

            var floor = new Cube
            {
                Material =
                {
                    Texture = text,
                    SpecularColor = new Color(0.3f,0.3f,0.3f),
                    Metallic = 0f,
                    Roughness = 0.45f,
                    Ambient = 0.15f
                }
            };
            floor.SetTransform(Transform.TranslateY(-1f).Scale(40f));

            var min = g.LocalBounds().Min;
            var max = g.LocalBounds().Max;
            var dir = max - min;
            var mid = min + (dir * 0.5f);

            var g2 = new Group(g, floor, skySphere);
            //var g2 = new Group(g);
            g2.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(mid.X, 500, -500), new Color(1.7f,1.7f,1.7f)));
            w.SetObjects(g2);

            //var width = 1200;
            //var height = 140;
            //var transform = Transforms.View(new Point(mid.X, 6f, -32f), mid, new Vector(0, 1, 0));
            //var c = new PinholeCamera(transform, MathF.PI / 4f, width, height);
            ////var c = new ApertureCamera(MathF.PI / 4f, width, height, 0.04f, new Point(mid.X, 6f, -32f), mid);
            //var ws = new ComposableWorldShading(3, GGXNormalDistribution.Instance, GGXSmithGeometricShadow.Instance, SchlickFresnelFunction.Instance, w);
            ////var ws = new PhongWorldShading(3, w);
            //var scene = new Scene(c, ws);
            //var aaa = new AdaptiveRenderer(4, 0.00001f, scene);
            //var canvas = new Canvas(width, height);


            var width = 1200;
            var height = 140;
            var from = new Point(mid.X, 6f, -32f);
            var to = mid;

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(400);
            var fov = System.MathF.PI / 4f;
            var aspectRatio = (float)width / height;
            var camera = new ApertureCamera(fov, aspectRatio, 0.2F, from, to, Vectors.Up);
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
            //RenderContext.Render(canvas, aaa);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "col_row");
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        public static void RowTestByDelegate(int spp, string fileSuffix, Func<int, MaterialInfo> materialFunc)
        {
            Console.WriteLine("Loading file...");
            var filePath = Path.Combine(GetExecutionPath(), "winter_river_1k.ppm");
            Console.WriteLine("Parsing file...");
            var textureCanvas = PPM.ParseFile(filePath);
            var image = new UVImage(textureCanvas);
            var map = new TextureMap(image, UVMapping.Spherical);

            var skySphere = new Sphere
            {
                Material = { Texture = map, Ambient = 1.5f, CastsShadows = false, Transparency = 1f }
            };

            skySphere.SetTransform(Transform.RotateY(3.4f).Scale(1000f));

            var g = new Group();
            var dx = 2.75f;
            var dz = 3.5f;
            var y = 1f;
            var nX = 10;
            var nZ = 1;
            int n = 0;
            for (var z = 0; z < nZ; z++)
            {
                for (var x = 0; x < nX; x++)
                {
                    var s = new Sphere();
                    s.SetTransform(Transform.TranslateY(1f).Scale(1.2f).Translate(x * dx, 0, z * dz));
                    s.SetMaterial(materialFunc(x + 1));
                    g.AddChild(s);
                    n++;
                }
            }

            var lightGray = new Color(0.9f, 0.9f, 0.9f);
            var darkGray = new Color(0.1f, 0.9f, 0.1f);
            var s1 = new StripeTexture(lightGray, darkGray);
            var s2 = new StripeTexture(lightGray, darkGray);
            s2.SetTransform(Transform.RotateY(System.MathF.PI / 2));
            var pattern = new BlendedCompositeTexture(s1, s2);
            pattern.SetTransform(Transform.Scale(1f / 30f));

            var text = new CheckerTexture(new Color(0.3f, 0.7f, 0.3f), new Color(0.13f, 0.13f, 0.13f));
            text.SetTransform(Transform.Scale(1f / 16f));

            var floor = new Cube
            {
                Material =
                {
                    Texture = text,
                    SpecularColor = new Color(0.3f,0.3f,0.3f),
                    Metallic = 0f,
                    Roughness = 0.45f,
                    Ambient = 0.15f
                }
            };
            floor.SetTransform(Transform.TranslateY(-1f).Scale(40f));

            var min = g.LocalBounds().Min;
            var max = g.LocalBounds().Max;
            var dir = max - min;
            var mid = min + (dir * 0.5f);

            var g2 = new Group(g, floor, skySphere);
            g2.Divide(1);

            var w = new World();
            w.SetLights(new PointLight(new Point(mid.X, 500, -500), new Color(1.7f, 1.7f, 1.7f)));
            w.SetObjects(g2);

            var width = 1200;
            var height = 140;
            var from = new Point(mid.X, 6f, -32f);
            var to = mid;

            var canvas = new Canvas(width, height);
            var pps = new PerPixelSampler(spp);
            var fov = System.MathF.PI / 4f;
            var aspectRatio = (float)width / height;
            var camera = new ApertureCamera(fov, aspectRatio, 0.2F, from, to, Vectors.Up);
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
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "col_row_"+ fileSuffix);
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}