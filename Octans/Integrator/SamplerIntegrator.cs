using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Octans.Memory;
using Octans.Reflection;
using Octans.Sampling;
using static System.Math;
using static System.MathF;

namespace Octans.Integrator
{
    public abstract class SamplerIntegrator : IIntegrator
    {
        private readonly ICamera _camera;

        private readonly ThreadLocal<ObjectArena> _perThreadArena =
            new ThreadLocal<ObjectArena>(() => new ObjectArena(), true);

        private readonly PixelArea _pixelBounds;
        private readonly ISampler _sampler;

        protected SamplerIntegrator(ICamera camera, ISampler sampler, in PixelArea pixelBounds)
        {
            _camera = camera;
            _sampler = sampler;
            _pixelBounds = pixelBounds;
        }

        public void Render(IScene scene)
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
#if DEBUG
            foreach (var a in queue)
            {
                RenderTile(a, nTiles.X, _sampler, _camera, scene, _pixelBounds, _perThreadArena.Value);
            }
#else
            Parallel.ForEach(
                queue, a => RenderTile(a, nTiles.X, _sampler, _camera, scene, _pixelBounds, _perThreadArena.Value));

#endif
            foreach (var arena in _perThreadArena.Values)
            {
                arena.Clear();
            }

            _camera.Film.WriteFile(1f);
        }

        private void RenderTile(in PixelArea tile,
                                int nTilesX,
                                ISampler sampler,
                                ICamera camera,
                                IScene scene,
                                PixelArea pixelBounds,
                                ObjectArena arena)
        {
            var seed = tile.Min.Y * nTilesX + tile.Min.X;
            var tileSampler = sampler.Clone(seed, arena);

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
                    var cameraSample = tileSampler.GetCameraSample(in pixel, arena);

                    var rayWeight = camera.GenerateRayDifferential(cameraSample, arena, out var ray);
                    ray.ScaleDifferentials(1f / Sqrt(tileSampler.SamplesPerPixel));

                    var L = Spectrum.Zero;

                    if (rayWeight > 0f)
                    {
                        L = Li(ray, scene, tileSampler, arena);
                    }

                    if (L.HasNaN())
                    {
                        Debug.Print("Not-a-number encountered in radiance value.");
                        L = Spectrum.Zero;
                    }
                    else if (L.YComponent() < -1e-5f)
                    {
                        Debug.Print("Negative luminance encountered.");
                        L = Spectrum.Zero;
                    }
                    else if (float.IsInfinity(L.YComponent()))
                    {
                        Debug.Print("Infinite luminance encountered.");
                        L = Spectrum.Zero;
                    }

                    filmTile.AddSample(cameraSample.FilmPoint, L, rayWeight);

                    arena.Reset();
                } while (tileSampler.StartNextSample());
            }

            camera.Film.MergeFilmTile(filmTile);
        }

        protected abstract Spectrum Li(
            in RayDifferential ray,
            IScene scene,
            ISampler tileSampler,
            IObjectArena arena,
            int depth = 0);

        protected abstract void Preprocess(in IScene scene, ISampler sampler);

        protected Spectrum SpecularReflect(
            RayDifferential ray,
            SurfaceInteraction si,
            IScene scene,
            ISampler sampler,
            IObjectArena arena,
            in int depth)
        {
            var wo = si.Wo;
            var type = BxDFType.Reflection | BxDFType.Specular;
            var f = si.BSDF.Sample_F(wo, out var wi, sampler.Get2D(), out var pdf, type, out _);

            var ns = si.ShadingGeometry.N;
            if (!(pdf > 0f) || f.IsBlack() || System.MathF.Abs(wi % ns) == 0f)
            {
                return Spectrum.Zero;
            }

            var rd = new RayDifferential(si.SpawnRay(wi));
            if (ray.HasDifferentials)
            {
                rd.HasDifferentials = true;
                rd.RxOrigin = si.P + si.Dpdx;
                rd.RyOrigin = si.P + si.Dpdy;

                var dndx = si.ShadingGeometry.Dndu * si.Dudx +
                           si.ShadingGeometry.Dndv * si.Dvdx;

                var dndy = si.ShadingGeometry.Dndu * si.Dudy +
                           si.ShadingGeometry.Dndv * si.Dvdy;

                var dwodx = -ray.RxDirection - wo;
                var dwody = -ray.RyDirection - wo;
                var dDNdx = (dwodx % ns) + (wo % dndx);
                var dDNdy = (dwody % ns) + (wo % dndy);

                rd.RxDirection = wi - dwodx + 2f * (Vector) ((wo % ns) * dndx + dDNdx * ns);
                rd.RyDirection = wi - dwody + 2f * (Vector) ((wo % ns) * dndy + dDNdy * ns);
            }

            return f * Li(rd, scene, sampler, arena, depth + 1) * System.MathF.Abs(wi % ns) / pdf;
        }

        protected Spectrum SpecularTransmit(
            in RayDifferential ray,
            SurfaceInteraction si,
            IScene scene,
            ISampler sampler,
            IObjectArena arena,
            in int depth)
        {
            var wo = si.Wo;
            var p = si.P;
            var bsdf = si.BSDF;
            var f = bsdf.Sample_F( wo, out var wi, sampler.Get2D(), out var pdf,
                          BxDFType.Transmission | BxDFType.Specular, out _);

            var ns = si.ShadingGeometry.N;
            if (!(pdf > 0f) || f.IsBlack() || Vector.AbsDot(wi, ns) == 0f)
            {
                return Spectrum.Zero;
            }

            var rd = new RayDifferential(si.SpawnRay(wi));
            if (ray.HasDifferentials)
            {
                rd.HasDifferentials = true;
                rd.RxOrigin = p + si.Dpdx;
                rd.RyOrigin = p + si.Dpdy;

                var dndx = si.ShadingGeometry.Dndu * si.Dudx +
                           si.ShadingGeometry.Dndv * si.Dvdx;
                var dndy = si.ShadingGeometry.Dndu * si.Dudy +
                           si.ShadingGeometry.Dndv * si.Dvdy;

                var eta = 1f / bsdf.Eta;
                if ((wo % ns) < 0f)
                {
                    eta = 1f / eta;
                    ns = -ns;
                    dndx = -dndx;
                    dndy = -dndy;
                }

                var dwodx = -ray.RxDirection - wo;
                var dwody = -ray.RyDirection - wo;
                var dDNdx = (dwodx % ns) + (wo % dndx);
                var dDNdy = (dwody % ns) + (wo % dndy);

                var mu = eta * (wo % ns) - Vector.AbsDot(wi, ns);
                var dmudx = (eta - eta * eta * (wo % ns)) / Vector.AbsDot(wi, ns) * dDNdx;
                var dmudy = (eta - eta * eta * (wo % ns)) / Vector.AbsDot(wi, ns) * dDNdy;

                rd.RxDirection = wi - eta * dwodx + (Vector) (mu * dndx + dmudx * ns);
                rd.RyDirection = wi - eta * dwody + (Vector) (mu * dndy + dmudy * ns);
            }

            return f * Li(rd, scene, sampler, arena, depth + 1) * System.MathF.Abs(wi % ns) / pdf;
        }
    }
}