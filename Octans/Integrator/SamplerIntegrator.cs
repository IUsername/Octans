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
        private readonly ISampler2 _sampler;

        protected SamplerIntegrator(ICamera camera, ISampler2 sampler, in PixelArea pixelBounds)
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
                    ray.ScaleDifferentials(1f / Sqrt(tileSampler.SamplesPerPixel));

                    var L = arena.Create<SpectrumAccumulator>().Zero();

                    if (rayWeight > 0f)
                    {
                        Li(L, ray, scene, tileSampler, arena);
                    }

                    if (L.HasNaN())
                    {
                        Debug.Print("Not-a-number encountered in radiance value.");
                        L.Zero();
                    }
                    else if (L.YComponent() < -1e-5f)
                    {
                        Debug.Print("Negative luminance encountered.");
                        L.Zero();
                    }
                    else if (float.IsInfinity(L.YComponent()))
                    {
                        Debug.Print("Infinite luminance encountered.");
                        L.Zero();
                    }

                    filmTile.AddSample(cameraSample.FilmPoint, L, rayWeight);

                    arena.Reset();
                } while (tileSampler.StartNextSample());
            }

            camera.Film.MergeFilmTile(filmTile);
        }

        protected abstract void Li(
            SpectrumAccumulator L,
            in RayDifferential ray,
            IScene scene,
            ISampler2 tileSampler,
            IObjectArena arena,
            int depth = 0);

        protected abstract void Preprocess(in IScene scene, ISampler2 sampler);

        protected void SpecularReflect(
            SpectrumAccumulator L,
            RayDifferential ray,
            SurfaceInteraction si,
            IScene scene,
            ISampler2 sampler,
            IObjectArena arena,
            in int depth)
        {
            var wo = si.Wo;
            var type = BxDFType.Reflection | BxDFType.Specular;
            var f = si.BSDF.Sample_F(wo, out var wi, sampler.Get2D(), out var pdf, type, out _);

            var ns = si.ShadingGeometry.N;
            if (!(pdf > 0f) || f.IsBlack() || System.MathF.Abs(wi % ns) == 0f)
            {
                return;
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
                var dDNdx = dwodx % ns + wo % dndx;
                var dDNdy = dwody % ns + wo % dndy;

                rd.RxDirection = wi - dwodx + 2f * (Vector) (wo % ns * dndx + dDNdx * ns);
                rd.RyDirection = wi - dwody + 2f * (Vector) (wo % ns * dndy + dDNdy * ns);
            }

            var nL = arena.Create<SpectrumAccumulator>().Zero();
            Li(nL, rd, scene, sampler, arena, depth + 1);
            if (!nL.IsBlack())
            {
                nL.Scale(System.MathF.Abs(wi % ns) / pdf);
                nL.Scale(f);
                L.Contribute(nL);
            }
        }

        protected void SpecularTransmit(
            SpectrumAccumulator L,
            in RayDifferential ray,
            SurfaceInteraction si,
            IScene scene,
            ISampler2 sampler,
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
                return;
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
                if (wo % ns < 0f)
                {
                    eta = 1f / eta;
                    ns = -ns;
                    dndx = -dndx;
                    dndy = -dndy;
                }

                var dwodx = -ray.RxDirection - wo;
                var dwody = -ray.RyDirection - wo;
                var dDNdx = dwodx % ns + wo % dndx;
                var dDNdy = dwody % ns + wo % dndy;

                var mu = eta * wo % ns - Vector.AbsDot(wi, ns);
                var dmudx = (eta - eta * eta * wo % ns) / Vector.AbsDot(wi, ns) * dDNdx;
                var dmudy = (eta - eta * eta * wo % ns) / Vector.AbsDot(wi, ns) * dDNdy;

                rd.RxDirection = wi - eta * dwodx + (Vector) (mu * dndx + dmudx * ns);
                rd.RyDirection = wi - eta * dwody + (Vector) (mu * dndy + dmudy * ns);
            }

            var nL = arena.Create<SpectrumAccumulator>().Zero();
            Li(nL, rd, scene, sampler, arena, depth + 1);
            if (!nL.IsBlack())
            {
                nL.Scale(f);
                nL.Scale(Vector.AbsDot(wi, ns) / pdf);
                L.Contribute(nL);
            }
        }
    }
}