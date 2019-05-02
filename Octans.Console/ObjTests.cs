using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Octans.Accelerator;
using Octans.Camera;
using Octans.Filter;
using Octans.Integrator;
using Octans.IO;
using Octans.Light;
using Octans.Material;
using Octans.Primitive;
using Octans.Sampling;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    public static class ObjTests
    {
        public static void TestRender(int spp, int height)
        {
            var aspectRatio = 1f;
            var width = (int) (height * aspectRatio);
            var from = new Point(278, 278, -700f);
            var to = new Point(278, 278, 0);

            var fov = MathF.Deg(278f / 400f);

            //var transform = Transform.LookAt2(from, to, Vectors.Up);
            var transform = Transform.Translate(278, 278, -700);
            var dist = Point.Distance(from, to);

            var filter = new MitchellFilter(new Vector2(2.5f, 2.5f), 0.8f, 0.1f);
            var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            var camera = PerspectiveCamera.Create(transform, aspectRatio, 0.8f, dist, fov, film);

            //var integrator = new AmbientOcclusionIntegrator(true, 128, camera,
            //                                                new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            //var integrator = new DepthIntegrator(700f, 1000f, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            //var integrator = new NormalIntegrator(camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            //var integrator = new WhittedIntegrator(2, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                       film.CroppedBounds);

            var integrator = new PathIntegrator(5, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                film.CroppedBounds, 0.5f, LightSampleStrategy.Uniform);

            film.SetSink(new Sink(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "tri"));

            var scene = Build();

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            integrator.Render(scene);
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        private static IScene Build()
        {
            var white =
                new MatteMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {1f, 1f, 1f}, SpectrumType.Illuminant)),
                    new ConstantTexture<float>(0), null);

            var red =
                new DisneyMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.9f, 0f, 0f}, SpectrumType.Reflectance)),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1.5f),
                    new ConstantTexture<float>(0.6f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.9f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.5f),
                    null);

            var yellow =
                new DisneyMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {1f, 1f, 0f}, SpectrumType.Reflectance)),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1.5f),
                    new ConstantTexture<float>(1f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.9f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.5f),
                    null);

            var cyan =
                new DisneyMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.01f, 0.5f, 0.01f}, SpectrumType.Reflectance)),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1.8f),
                    new ConstantTexture<float>(1f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.9f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0.5f),
                    null);

            var metal = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.5f, 0.5f, 0.5f}, SpectrumType.Reflectance)),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(1.8f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<Spectrum>(Spectrum.Zero),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.5f),
                null);

            var sss = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.9f, 0.9f, 0.9f}, SpectrumType.Reflectance)),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1.5f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<Spectrum>(new Spectrum(10f)),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.5f),
                null);

            var mirror =
                new MirrorMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.8f, 0.8f, 0.8f}, SpectrumType.Illuminant)),
                    null);

            var trans = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.9f, 0.9f, 0.9f}, SpectrumType.Illuminant)),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(1.62f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0.90f),
                new ConstantTexture<Spectrum>(new Spectrum(0f)),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                null);

            var path = Path.Combine(TestScenes.GetExecutionPath(), "teapot.obj");
            Console.WriteLine("Loading file {0}...", path);
            var data = ObjFileMeshBuilder.ParseFile(path);
            Console.WriteLine("File parsed...");

            var teapotTransform = Transform.Scale(10)
                                           .RotateX(MathF.Rad(-90))
                                           .RotateY(MathF.Rad(26))
                                           .Translate(272f, 175f, -123f);
            var teapot = data.Meshes[0].BuildShape(teapotTransform, Transform.Invert(teapotTransform), false);
            var primitives = teapot.Select(tri => new GeometricPrimitive(tri, trans, null)).Cast<IPrimitive>().ToList();

            var plane = TestScenes.CreatePlane(new Point(-1500, 178, -800), new Point(1500, 178, 2500));

            primitives.AddRange(plane.Select(tri => new GeometricPrimitive(tri, cyan, null)));

            var bvh = new BVH(primitives.ToArray(), SplitMethod.HLBVH);

            var transform = Transform.RotateX(MathF.Rad(70)).Translate(278, 600, -200);
            var spectrum = Spectrum.FromBlackbodyT(4000) * 500000f;
            var spotLight = new SpotLight(transform, null, spectrum, 120, 20);

            transform = Transform.Translate(272,200, -123);
            spectrum = Spectrum.FromBlackbodyT(7500) * 20000f;
            var pointLight = new PointLight(transform, null, spectrum);

            return new Scene(bvh, new ILight[] {pointLight, spotLight});
        }
    }
}