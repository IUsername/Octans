using System;
using System.Diagnostics;
using System.Numerics;
using Octans.Accelerator;
using Octans.Camera;
using Octans.Filter;
using Octans.Integrator;
using Octans.IO;
using Octans.Material;
using Octans.Primitive;
using Octans.Sampling;
using Octans.Texture;

namespace Octans.ConsoleApp
{
    public static class IntegratorTests
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
            var s6g = new GeometricPrimitive(s6, red, null);

            var bvh = new BVH(new IPrimitive[] {s1g, s2g, s3g, s4g, s5g, s6g}, SplitMethod.HLBVH);

            var lt = Transform.Translate(400, 900, -800);
            var s = Spectrum.FromBlackbodyT(5500) * 2000000f;
            var pointLight = new PointLight2(lt, s);

            return new Scene(bvh, new ILight2[] {pointLight});
        }
    }

    public class Sink : ISinkRgb
    {
        private readonly string _fileName;
        private readonly string _path;

        public Sink(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
        }

        public void Write(in ReadOnlySpan<float> rgb, in PixelArea area, in PixelVector fullResolution)
        {
            PPM.ToFile(rgb, fullResolution, _path, _fileName);
        }
    }
}