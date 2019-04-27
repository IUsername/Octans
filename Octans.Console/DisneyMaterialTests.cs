using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Octans.Accelerator;
using Octans.Camera;
using Octans.Filter;
using Octans.Integrator;
using Octans.Material;
using Octans.Primitive;
using Octans.Sampling;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    public static class DisneyMaterialTests
    {
        public static void TestRender(int spp, int height)
        {
            var aspectRatio = 1f;
            var width = (int) (height * aspectRatio);
            var from = new Point(278, 278, -800f);
            var to = new Point(278, 278, 0);

            var fov = MathF.Deg(278f / 400f);

            //var transform = Transform.LookAt2(from, to, Vectors.Up);
            var transform = Transform.Translate(278, 278, -800);
            var dist = Point.Distance(from, to);

            var filter = new MitchellFilter(new Vector2(2f, 2f), 0.5f, 0.25f);
            var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            var camera = PerspectiveCamera.Create(transform, aspectRatio, 0.8f, dist, fov, film);

            //var integrator = new AmbientOcclusionIntegrator(true, 64, camera,
            //                                                new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            //var integrator = new DepthIntegrator(700f, 1000f, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            //var integrator = new NormalIntegrator(camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            var integrator = new WhittedIntegrator(5, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                   film.CroppedBounds);

            film.SetSink(new Sink(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "int"));

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
                new PlasticMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {1f, 0f, 0f}, SpectrumType.Illuminant)),
                    new ConstantTexture<Spectrum>(new Spectrum(0.3f)),
                    new ConstantTexture<float>(0.0f),
                    null,
                    true);

            var yellow =
                new MatteMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {1f, 1f, 0f}, SpectrumType.Illuminant)),
                    new ConstantTexture<float>(0), null);

            var cyan =
                new MatteMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0f, 1f, 1f}, SpectrumType.Illuminant)),
                    new ConstantTexture<float>(0), null);

            var metal =
                new MetalMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.8f, 0.8f, 0.8f}, SpectrumType.Illuminant)),
                    new ConstantTexture<Spectrum>(Spectrum.One),
                    new ConstantTexture<float>(0.01f), null, null, null, true);

            var mirror =
                new MirrorMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.8f, 0.8f, 0.8f}, SpectrumType.Illuminant)),
                    null);

            var dm = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {1f, 0f, 0f}, SpectrumType.Illuminant)),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0.4f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<float>(0.6f),
                new ConstantTexture<float>(0.8f),
                new ConstantTexture<float>(0.0f),
                new ConstantTexture<Spectrum>(Spectrum.Zero),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                null);


            var s1t = Transform.Translate(278, 278, 100);
            var s1 = new Sphere(s1t, Transform.Invert(s1t), false, 100f, -100, 100, 360);
            var s1g = new GeometricPrimitive(s1, mirror, null);

            var s2t = Transform.Translate(340, 100, 80);
            var s2 = new Sphere(s2t, Transform.Invert(s2t), false, 120f, -120, 120, 360);
            var s2g = new GeometricPrimitive(s2, cyan, null);

            var s3t = Transform.Translate(0, 278, 200);
            var s3 = new Sphere(s3t, Transform.Invert(s3t), false, 300f, -300, 300, 360);
            var s3g = new GeometricPrimitive(s3, yellow, null);

            var s4t = Transform.Translate(400, 500, 150);
            var s4 = new Sphere(s4t, Transform.Invert(s4t), false, 180f, -180, 180, 360);
            var s4g = new GeometricPrimitive(s4, metal, null);

            var s5t = Transform.Translate(420, 200, 250);
            var s5 = new Sphere(s5t, Transform.Invert(s5t), false, 180f, -180, 180, 360);
            var s5g = new GeometricPrimitive(s5, white, null);

            var s6t = Transform.Translate(500, 30, 180);
            var s6 = new Sphere(s6t, Transform.Invert(s6t), false, 200f, -200, 200, 360);
            var s6g = new GeometricPrimitive(s6, dm, null);

            var bvh = new BVH(new IPrimitive[] {s1g, s2g, s3g, s4g, s5g, s6g}, SplitMethod.HLBVH);

            var lt = Transform.Translate(400, 900, -800);
            var s = Spectrum.FromBlackbodyT(5500) * 2000000f;
            var pointLight = new PointLight2(lt, s);

            return new Scene(bvh, new ILight2[] {pointLight});
        }

        public static void PlasticToMetal(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn", (f,i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] {0.9f, 0.9f, 0.9f}, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] {0.2f, 0.2f, 0.8f}, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(0.74f),
                    roughness: new ConstantTexture<float>(f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(f),
                    sheenTint: new ConstantTexture<float>(0f),
                    clearcoat: new ConstantTexture<float>(1f - f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void RowTestByDelegate(int spp, string fileSuffix, Func<float, int, IMaterial> materialFunc)
        {
            var dx = 3.65f;
            var nX = 11;
            var delta = 1f / (nX - 1);
            var prims = new List<IPrimitive>();

            for (var x = 0; x < nX; x++)
            {
                var s1t = Transform.Translate(x * dx, 0, 0);
                var s1 = new Sphere(s1t, Transform.Invert(s1t), false, 1.5f, -1.5f, 1.5f, 360);
                var s1g = new GeometricPrimitive(s1, materialFunc(delta * x, x+1), null);
                prims.Add(s1g);
            }

            var bvh = new BVH(prims.ToArray(), SplitMethod.EqualCounts);

            var mid = bvh.WorldBounds.Centroid;

            var width = 1200;
            var height = 140;
            var from = new Point(mid.X, 6f, -12f);
            var to = mid;

            var fov = 5.6f;
            var aspectRatio = (float) width / height;


            var transform = Transform.Translate(mid.X, 0f, -50f);
            var dist = Point.Distance(from, to);
            var filter = new MitchellFilter(new Vector2(2.25f, 2.25f), 0.9f, 0.05f);
            var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            var camera = PerspectiveCamera.Create(transform, aspectRatio, 0.0f, dist, fov, film);

            //var integrator = new WhittedIntegrator(2, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                       film.CroppedBounds);

            var integrator = new PathIntegrator(2, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                   film.CroppedBounds, 3f, LightSampleStrategy.Spatial);

            film.SetSink(new Sink(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                                  "row_" + fileSuffix));

            var lt = Transform.Translate(mid.X-60, 100, -40);
            var s = Spectrum.FromBlackbodyT(4500) * 30000f;
            var pl1 = new PointLight2(lt, s);

            lt = Transform.Translate(mid.X + 100, 100, -200);
            s = Spectrum.FromBlackbodyT(2200) * 100000f;
            var pl2 = new PointLight2(lt, s);

            lt = Transform.Translate(mid.X, -20, 100);
            s = Spectrum.FromBlackbodyT(7000) * 5000f;
            var pl3 = new PointLight2(lt, s);

            var scene = new Scene(bvh, new ILight2[] {pl1, pl2, pl3});

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            integrator.Render(scene);
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}