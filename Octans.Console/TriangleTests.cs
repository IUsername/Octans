using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Octans.Accelerator;
using Octans.Camera;
using Octans.Filter;
using Octans.Integrator;
using Octans.Light;
using Octans.Material;
using Octans.Primitive;
using Octans.Sampling;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    public static class TriangleTests
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

            //var integrator = new WhittedIntegrator(5, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                       film.CroppedBounds);

            var integrator = new PathIntegrator(5, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                film.CroppedBounds, 1f, LightSampleStrategy.Uniform);

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
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.6f, 0f, 0.9f}, SpectrumType.Illuminant)),
                    new ConstantTexture<float>(0), null);

            var sss = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] { 0.9f, 0.9f, 0.9f }, SpectrumType.Reflectance)),
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
                new ConstantTexture<Spectrum>(new Spectrum(40f)),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0.5f),
                null);

            var red =
                new DisneyMaterial(
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.9f, 0f, 0f}, SpectrumType.Reflectance)),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1.5f),
                    new ConstantTexture<float>(0.8f),
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
                    new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0f, 1f, 1f}, SpectrumType.Reflectance)),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1.8f),
                    new ConstantTexture<float>(0.1f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1f),
                    new ConstantTexture<float>(0f),
                    new ConstantTexture<float>(1f),
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
                new ConstantTexture<float>(1.5f),
                new ConstantTexture<float>(0.4f),
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

            var mirror = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.7f, 0.7f, 0.8f}, SpectrumType.Reflectance)),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(1.5f),
                new ConstantTexture<float>(0.1f),
                new ConstantTexture<float>(0.5f),
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

            var trans = new DisneyMaterial(
                new ConstantTexture<Spectrum>(Spectrum.FromRGB(new[] {0.0f, 0.8f, 0.0f}, SpectrumType.Illuminant)),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(2.5f),
                new ConstantTexture<float>(0.01f),
                new ConstantTexture<float>(0.5f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(1f),
                new ConstantTexture<float>(0f),
                new ConstantTexture<Spectrum>(new Spectrum(0f)),
                false,
                new ConstantTexture<float>(0f),
                new ConstantTexture<float>(0f),
                null);

            var px = 3000;
            var pz = 3000;
            var tr = Transform.Translate(-500, 178, -600);
            var tris = TriangleMesh.CreateTriangleMesh(
                tr,
                Transform.Invert(tr),
                false,
                2,
                new[] {0, 1, 2, 3, 2, 1},
                4,
                new[]
                {
                    new Point(0, 0, 0),
                    new Point(0, 0, pz),
                    new Point(px, 0, 0),
                    new Point(px, 0, pz)
                },
                null,  
                null, null, null, null, null);

            var prims = new List<IPrimitive>();

            foreach (var tri in tris)
            {
                prims.Add(new GeometricPrimitive(tri, red, null));
            }

            var s1t = Transform.Translate(278, 278, 100);
            var s1 = new Sphere(s1t, Transform.Invert(s1t), false, 100f, -100, 100, 360);
            var s1g = new GeometricPrimitive(s1, sss, null);
            prims.Add(s1g);

            var s2t = Transform.Translate(390, 228, 30);
            var s2 = new Sphere(s2t, Transform.Invert(s2t), false, 50f, -50, 50, 360);
            var s2g = new GeometricPrimitive(s2, cyan, null);
            prims.Add(s2g);

            var s3t = Transform.Translate(50, 328, 200);
            var s3 = new Sphere(s3t, Transform.Invert(s3t), false, 150f, -150, 150, 360);
            var s3g = new GeometricPrimitive(s3, yellow, null);
            prims.Add(s3g);

            var s4t = Transform.Translate(480, 238, -50);
            var s4 = new Sphere(s4t, Transform.Invert(s4t), false, 60f, -60, 60, 360);
            var s4g = new GeometricPrimitive(s4, metal, null);
            prims.Add(s4g);

            var s5t = Transform.Translate(490, 255, 250);
            var s5 = new Sphere(s5t, Transform.Invert(s5t), false, 80f, -80, 80, 360);
            var s5g = new GeometricPrimitive(s5, white, null);
            prims.Add(s5g);

            var s7t = Transform.Translate(150, 248, -120);
            var s7 = new Sphere(s7t, Transform.Invert(s7t), false, 70f, -70, 70, 360);
            var s7g = new GeometricPrimitive(s7, trans, null);
            prims.Add(s7g);

            //var temp = 5000f;
            //var lightMatte =
            //    new MatteMaterial(
            //        new ConstantTexture<Spectrum>(Spectrum.FromBlackbodyT(temp)),
            //        new ConstantTexture<float>(0), null);
            //var s6t = Transform.Translate(400, 188, -230);
            //var s6 = new Sphere(s6t, Transform.Invert(s6t), false, 10f, -10, 10, 360);
            //var dl = new DiffuseAreaLight(s6t, null, Spectrum.FromBlackbodyT(temp) * 60f, 12, s6);
            //var s6g = new GeometricPrimitive(s6, lightMatte, dl);
            //prims.Add(s6g);

            var bvh = new BVH(prims.ToArray(), SplitMethod.HLBVH);

            var lt = Transform.RotateX(MathF.Rad(75)).Translate(278, 600, -120);
            var s = Spectrum.FromBlackbodyT(4500) * 400000f;
            var spot = new SpotLight(lt, null, s, 120, 20);

            lt = Transform.RotateX(MathF.Rad(75)).Translate(278, 300, -300);
            s = Spectrum.FromBlackbodyT(6500) * 55000f;
            var point = new PointLight(lt, null, s);

            return new Scene(bvh, new ILight[] {spot, point});
        }
    }
}