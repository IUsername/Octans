﻿using System;
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
            var w = Build();

            var aspectRatio = 1f;
            var width = (int)(height * aspectRatio);
            var from = new Point(278, 278, -800f);
            var to = new Point(278, 278, 0);

            var fov = MathF.Deg(278f / 400f);

            //var transform = Transform.LookAt2(from, to, Vectors.Up);
            var transform = Transform.Translate(278, 278, -800);
            var dist = Point.Distance(from, to);

            var filter = new MitchellFilter(new Vector2(1f, 1f), 0.5f, 0.25f);
            var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            var camera = PerspectiveCamera.Create(transform, aspectRatio, 0f, dist, fov, film);

            //var integrator = new AmbientOcclusionIntegrator(true, spp, camera,
            //                                                new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);

            var integrator = new DepthIntegrator(800f, 900f, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                            film.CroppedBounds);

            film.SetSink(new Sink(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "int"));
            var scene = new Scene(w, new ILight[0]);

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            integrator.Render(scene);
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }

        private static IPrimitive Build()
        {
            var material = new MatteMaterial(new ConstantTexture<Spectrum>(Spectrum.One),new ConstantTexture<float>(0f), null);

            var s1t = Transform.Translate(278, 278, 100);
            var s1 = new Sphere(s1t, Transform.Invert(s1t), false, 100f, -100, 100, 360);
            var s1g = new GeometricPrimitive(s1, material, null);

            var s2t = Transform.Translate(440, 278, 80);
            var s2 = new Sphere(s2t, Transform.Invert(s2t), false, 30f, -30, 30, 360);
            var s2g = new GeometricPrimitive(s2, material, null);

            var s3t = Transform.Translate(140, 278, 100);
            var s3 = new Sphere(s3t, Transform.Invert(s3t), false, 60f, -60, 60, 360);
            var s3g = new GeometricPrimitive(s3, material, null);

            var bvh = new BVH(new IPrimitive[] {s1g, s2g, s3g}, SplitMethod.EqualCounts);

            return bvh;
        }
    }

    public class Sink : ISinkRgb
    {
        private readonly string _path;
        private readonly string _fileName;

        public Sink(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
        }

        public void Write(in ReadOnlySpan<float> rgb, in PixelArea area, in PixelVector fullResolution)
        {
            PPM.ToFile(rgb, fullResolution,  _path, _fileName);
        }
    }
}