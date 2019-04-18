using System.Numerics;
using Octans.Camera;
using Octans.Filter;
using Octans.Integrator;
using Octans.Sampling;

namespace Octans.ConsoleApp
{
    public static class IntegratorTests
    {
        public static void TestRender(int spp, int height)
        {
            //var w = BuildBox();

            //var aspectRatio = 1f;
            //var width = (int)(height * aspectRatio);
            //var from = new Point(278, 278, -800f);
            //var to = new Point(278, 278, 0);

            //var fov = 278f / 400f;

            //var transform = Transform.LookAt2(from, to, Vectors.Up);
            //var dist = Point.Distance(from, to);
       
            //var filter = new MitchellFilter(new Vector2(2f, 2f), 0.5f, 0.25f);
            //var film = new Film(new PixelVector(width, height), new Bounds2D(0, 0, 1, 1), filter, 20f, 1f);
            //var camera = PerspectiveCamera.Create(Transform.Invert(transform), aspectRatio, 0f, dist, fov, film);

            //var integrator = new AmbientOcclusionIntegrator(true, spp, camera,
            //                                                new HaltonSampler(spp, film.GetSampleBounds()),
            //                                                film.CroppedBounds);
            //var scene = new Scene(w.Objects[0], w.Lights.ToArray());
            //integrator.Render(scene);
        }
    }
}