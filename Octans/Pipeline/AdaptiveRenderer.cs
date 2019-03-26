namespace Octans.Pipeline
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

            var shared = samples as ISharedPixelSamples;
            if (shared != null)
            {
                // No need to keep color values contained within pixel edges.
                // Create local scope that will be closed when pixel is fully rendered.
                samples = shared.CreateLocalScope();
            }

            var changed = false;
            if (!brc)
            {
                cbr = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in br, in c));
                avg = (ctl + ctr + cbl + cbr) / 4f;
                changed = true;
            }

            if (!changed && !blc || !Color.IsWithinDelta(in cbl, in avg, maxDelta))
            {
                cbl = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in bl, in c));
                avg = (ctl + ctr + cbl + cbr) / 4f;
                changed = true;
            }

            if (!changed && !trc || !Color.IsWithinDelta(in ctr, in avg, maxDelta))
            {
                ctr = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in tr, in c));
                avg = (ctl + ctr + cbl + cbr) / 4f;
                changed = true;
            }

            if (changed || !Color.IsWithinDelta(in ctl, in avg, maxDelta))
            {
                ctl = RenderSubPixel(r, maxDelta, samples, renderer, SubPixel.Center(in tl, in c));
                avg = (ctl + ctr + cbl + cbr) / 4f;
            }

            shared?.CloseLocalScope(samples);
            return avg;
        }
    }
}