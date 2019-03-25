using System.ComponentModel.DataAnnotations;

namespace Octans
{
    public sealed class AdaptiveRenderer : IPixelRenderer
    {
        private readonly IPixelRenderer _renderer;

        private readonly PixelSamples _samples;

        public AdaptiveRenderer(int maxPasses, float maxDelta, IPixelRenderer renderer)
        {
            _renderer = renderer;
            MaxPasses = maxPasses;
            MaxDelta = maxDelta;
            _samples = new PixelSamples();
        }

        public int MaxPasses { get; }
        public float MaxDelta { get; }

        public Color Render(in SubPixel sp) => RenderSubPixel(MaxPasses, MaxDelta, _samples, _renderer, in sp);

        private static Color RenderSubPixel(int remaining,
                                            float maxDelta,
                                            IPixelSamples samples,
                                            IPixelRenderer renderer,
                                            in SubPixel sp)
        {
            if (remaining < 1)
            {
                return renderer.Render(in sp);
            }

            var (tl, tr, bl, br) = SubPixel.Corners(in sp);

            var cbr = samples.GetOrAdd(br, renderer);
            var cbl = samples.GetOrAdd(bl, renderer);
            var ctr = samples.GetOrAdd(tr, renderer);
            var ctl = samples.GetOrAdd(tl, renderer);

            var avg = (ctl + ctr + cbl + cbr) / 4f;

            if (remaining < 2)
            {
                return avg;
            }

            var tlc = Color.IsWithinDelta(in ctl, in avg, maxDelta);
            var trc = Color.IsWithinDelta(in ctr, in avg, maxDelta);
            var blc = Color.IsWithinDelta(in cbl, in avg, maxDelta);
            var brc = Color.IsWithinDelta(in cbr, in avg, maxDelta);

            if (tlc & trc & blc & brc)
            {
                return avg;
            }

            var c = SubPixel.Center(in tl, in br);
            var r = remaining - 1;

            //// Increase the tolerance to sample further?
            //var t = tolerance;// * 1.2f;

            var shared = samples as ISharedPixelSamples;
            if (shared != null)
            {
                // No need to keep color values contained within pixel edges.
                // Create local scope that will be closed when pixel is fully rendered.
                samples = shared.CreateLocalScope();
            }

            var cc = samples.GetOrAdd(c, renderer);
            var cwa = (ctl + ctr + cbl + cbr + cc) / 5f;

            if (!brc)
            {
                if (!Color.IsWithinDelta(in cbr, in cwa, maxDelta))
                {
                    cbr = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in br, in c));
                }
            }

            if (!blc)
            {
                if (!Color.IsWithinDelta(in cbl, in cwa, maxDelta))
                {
                    cbl = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in bl, in c));
                }
            }

            if (!trc)
            {
                if (!Color.IsWithinDelta(in ctr, in cwa, maxDelta))
                {
                    ctr = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in tr, in c));
                }
            }

            if (!tlc)
            {
                if (!Color.IsWithinDelta(in ctl, in cwa, maxDelta))
                {
                    ctl = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in tl, in c));
                }
            }

            shared?.CloseLocalScope(samples);
            return (ctl + ctr + cbl + cbr) / 4f;
        }
    }
}