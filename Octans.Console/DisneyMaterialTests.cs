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
    public static class DisneyMaterialTests
    {
        public static void SubSurfaceDepth(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_sss_depth", (f,i) =>
            {
                var color = Spectrum.FromRGB(new[] {0.95f, 0.63f, 0.5f}, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.45f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(1f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(new Spectrum(f*2f)),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void PlasticToMetal(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_p2m", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.2f, 0.2f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.45f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(1f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void Clearcoat(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_clearcoat", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.2f, 0.2f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.45f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(1f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(f),
                    clearcoatGloss: new ConstantTexture<float>(0.95f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void RoughnessPlastic(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_roughness_p", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.0f, 0.0f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(f),
                    specularTint: new ConstantTexture<float>(1f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void EtaTest(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_eta", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.0f, 0.0f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(2f - f),
                    roughness: new ConstantTexture<float>(0.2f),
                    specularTint: new ConstantTexture<float>(0.5f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void RoughnessMetal(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_roughness_m", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(1f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void Translucency(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_trans_p", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.3f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0.5f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void Flatness(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_flattness", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.4f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    isThin: true,
                    flatness: new ConstantTexture<float>(f),
                    diffTrans: new ConstantTexture<float>(0f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void Anisotropic(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_anisotropic", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.9f, 0.2f, 0.2f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0.6f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.3f),
                    specularTint: new ConstantTexture<float>(0f),
                    anisotropic: new ConstantTexture<float>(f),
                    sheen: new ConstantTexture<float>(0f),
                    sheenTint: new ConstantTexture<float>(0.5f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    isThin: false,
                    flatness: new ConstantTexture<float>(0f),
                    diffTrans: new ConstantTexture<float>(0f),
                    null);
            });
            // ReSharper restore ArgumentsStyleOther
        }

        public static void Sheen(int spp)
        {
            // ReSharper disable ArgumentsStyleOther
            RowTestByDelegate(spp, "disn_sheen", (f, i) =>
            {
                var color = i % 2 == 0
                    ? Spectrum.FromRGB(new[] { 0.8f, 0.8f, 0.8f }, SpectrumType.Reflectance)
                    : Spectrum.FromRGB(new[] { 0.3f, 0.3f, 0.3f }, SpectrumType.Reflectance);
                return new DisneyMaterial(
                    color: new ConstantTexture<Spectrum>(color),
                    metallic: new ConstantTexture<float>(0f),
                    eta: new ConstantTexture<float>(1.5f),
                    roughness: new ConstantTexture<float>(0.8f),
                    specularTint: new ConstantTexture<float>(0.5f),
                    anisotropic: new ConstantTexture<float>(0f),
                    sheen: new ConstantTexture<float>(f),
                    sheenTint: new ConstantTexture<float>(0f),
                    clearcoat: new ConstantTexture<float>(0f),
                    clearcoatGloss: new ConstantTexture<float>(1f),
                    specTrans: new ConstantTexture<float>(0f),
                    scatterDistance: new ConstantTexture<Spectrum>(Spectrum.Zero),
                    isThin: false,
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

            var slt = Transform.Translate(5, 2, -4);
            var slg = new Sphere(slt, Transform.Invert(slt), false, 0.5f, -0.5f, 0.5f, 360);
            var ls = Spectrum.FromBlackbodyT(7500);
            var dl = new DiffuseAreaLight(slt, null, ls*100, 20, slg);
            var sg = new GeometricPrimitive(slg, new MatteMaterial(new ConstantTexture<Spectrum>(ls), new ConstantTexture<float>(0f), null ), dl);
            prims.Add(sg);

            var bvh = new BVH(prims.ToArray(), SplitMethod.HLBVH);

            var mid = bvh.WorldBounds.Centroid;

            var width = 1200;
            var height = 140;
            var from = new Point(mid.X, 6f, -12f);
            var to = mid;

            var fov = 5.6f;
            var aspectRatio = (float) width / height;

            var transform = Transform.Translate(mid.X, 0f, -50f);
            var dist = Point.Distance(from, to);
            var filter = new MitchellFilter(new Vector2(2.5f, 2.5f), 0.7f, 0.15f);
            var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            var camera = PerspectiveCamera.Create(transform, aspectRatio, 0.0f, dist, fov, film);

            //var integrator = new WhittedIntegrator(4, camera, new HaltonSampler(spp, film.GetSampleBounds()),
            //                                       film.CroppedBounds);

            var integrator = new PathIntegrator(3, camera, new HaltonSampler(spp, film.GetSampleBounds()),
                                                   film.CroppedBounds, 6f, LightSampleStrategy.Spatial);

            film.SetSink(new Sink(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "row_" + fileSuffix));

            var lt = Transform.Translate(mid.X-60, 200, -80);
            var s = Spectrum.FromBlackbodyT(4500) * 100000f;
            var pl1 = new PointLight(lt, null, s);

            lt = Transform.Translate(mid.X + 100, 100, -200);
            s = Spectrum.FromBlackbodyT(2200) * 50000f;
            var pl2 = new PointLight(lt, null, s);

            lt = Transform.Translate(mid.X, 1000f, 0);
            s = Spectrum.FromBlackbodyT(7000) * 5f;
            var pl3 = new DistantLight(lt, s, new Vector(0,-0.5f,1));

            lt = Transform.Translate(mid.X, 10, -300);
            s = Spectrum.FromBlackbodyT(5500) * 100000f;
            var pl4 = new PointLight(lt, null, s);

            var splt = Transform.Translate(mid.X, 0, -40).RotateX(MathF.Rad(0));
            var test = Transform.LookAt(new Point(mid.X, 0, -40), new Point(mid.X, 0, 0), Vectors.Up);
            s = Spectrum.FromBlackbodyT(5500) * 100000f;
            var sl = new SpotLight(splt, null, s, 2f, 1.5f);

            var scene = new Scene(bvh, new ILight[] {pl1, pl2,  pl3, pl4, dl, sl});
            //var scene = new Scene(bvh, new ILight[] { dl });

            Console.WriteLine("Rendering at {0}x{1}...", width, height);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            integrator.Render(scene);
            stopwatch.Stop();
            Console.WriteLine("Done ({0})", stopwatch.Elapsed);
        }
    }
}