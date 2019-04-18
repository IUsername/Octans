using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Octans.Memory;
using Octans.Sampling;
using static System.Math;

namespace Octans.Integrator
{
    public abstract class SamplerIntegrator : IIntegrator
    {
        private readonly ICamera _camera;

        private readonly ThreadLocal<ObjectArena> _perThreadArena =
            new ThreadLocal<ObjectArena>(() => new ObjectArena());

        private readonly PixelArea _pixelBounds;
        private readonly ISampler2 _sampler;

        protected SamplerIntegrator(ICamera camera, ISampler2 sampler, in PixelArea pixelBounds)
        {
            _camera = camera;
            _sampler = sampler;
            _pixelBounds = pixelBounds;
        }

        public void Render(Scene scene)
        {
            Preprocess(in scene, _sampler);

            var sampleBounds = _camera.Film.GetSampleBounds();
            var sampleExtent = sampleBounds.Diagonal();

            const int tileSize = 16;
            var nTiles = new PixelCoordinate(
                (sampleExtent.X + tileSize - 1) / tileSize,
                (sampleExtent.Y + tileSize - 1) / tileSize);

            var queue = new ConcurrentQueue<PixelArea>();
            for (var y = 0; y < nTiles.Y; ++y)
            {
                var y0 = sampleBounds.Min.Y + y * tileSize;
                var y1 = Min(sampleBounds.Max.Y, y0 + tileSize);
                for (var x = 0; x < nTiles.X; ++x)
                {
                    var x0 = sampleBounds.Min.X + x * tileSize;
                    var x1 = Min(sampleBounds.Max.X, x0 + tileSize);
                    var min = new PixelCoordinate(x0, y0);
                    var max = new PixelCoordinate(x1, y1);
                    queue.Enqueue(new PixelArea(min, max));
                }
            }

            Parallel.ForEach(
                queue, a => RenderTile(a, nTiles.X, _sampler, _camera, scene, _pixelBounds, _perThreadArena.Value));

            foreach (var arena in _perThreadArena.Values)
            {
                arena.Clear();
            }
        }

        private void RenderTile(in PixelArea tile,
                                int nTilesX,
                                ISampler2 sampler,
                                ICamera camera,
                                IScene scene,
                                PixelArea pixelBounds,
                                ObjectArena arena)
        {
            var seed = tile.Min.Y * nTilesX + tile.Min.X;
            var tileSampler = sampler.Clone(seed);

            var filmTile = camera.Film.CreateFilmTile(tile);

            foreach (var pixel in tile)
            {
                tileSampler.StartPixel(in pixel);

                if (!pixelBounds.InsideExclusive(in pixel))
                {
                    continue;
                }

                do
                {
                    var cameraSample = tileSampler.GetCameraSample(in pixel);

                    var rayWeight = camera.GenerateRayDifferential(cameraSample, out var ray);
                    //ray.ScaleDifferentials(1f / System.MathF.Sqrt(tileSampler.SamplesPerPixel));

                    var L = Spectrum.Zero;
                    ;
                    if (rayWeight > 0f)
                    {
                        L = Li(ray, scene, tileSampler, arena);
                    }

                    // TODO: Check for unexpected results.

                    filmTile.AddSample(cameraSample.FilmPoint, L, rayWeight);

                    arena.Reset();
                } while (tileSampler.StartNextSample());
            }

            camera.Film.MergeFilmTile(filmTile);

            camera.Film.WriteImage();
        }

        protected abstract Spectrum Li(in Ray ray, IScene scene, ISampler2 tileSampler, IObjectArena arena);

        protected abstract void Preprocess(in Scene scene, ISampler2 sampler);
    }

   
}