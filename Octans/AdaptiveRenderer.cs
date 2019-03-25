namespace Octans
{
    public sealed class AdaptiveRenderer : IPixelRenderer
    {
        private readonly IPixelRenderer _renderer;

        private readonly PixelSamples _samples;

        public AdaptiveRenderer(int maxPasses, float tolerance, IPixelRenderer renderer)
        {
            _renderer = renderer;
            MaxPasses = maxPasses;
            Tolerance = tolerance;
            _samples = new PixelSamples();
        }

        public int MaxPasses { get; }
        public float Tolerance { get; }

        public Color Render(in SubPixel sp) => RenderSubPixel(MaxPasses, Tolerance, _samples, _renderer, in sp);

        private static Color RenderSubPixel(int remaining,
                                            float tolerance,
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

            var tlc = Color.IsWithinDelta(in ctl, in avg, tolerance);
            var trc = Color.IsWithinDelta(in ctr, in avg, tolerance);
            var blc = Color.IsWithinDelta(in cbl, in avg, tolerance);
            var brc = Color.IsWithinDelta(in cbr, in avg, tolerance);

            if (tlc & trc & blc & brc)
            {
                return avg;
            }

            var c = SubPixel.Center(in tl, in br);
            var r = remaining - 1;

            // Increase the tolerance to sample further.
            var t = tolerance;// * 1.2f;

            var shared = samples as ISharedPixelSamples;
            if (shared != null)
            {
                // No need to keep color values contained within pixel edges.
                // Create local scope that will be closed when pixel is fully rendered.
                samples = shared.CreateLocalScope();
            }

            if (!brc)
            {
                cbr = RenderSubPixel(r, t, samples, renderer, SubPixel.Center(in br, in c));
            }

            if (!blc)
            {
                cbl = RenderSubPixel(r, t, samples, renderer, SubPixel.Center(in bl, in c));
            }

            if (!trc)
            {
                ctr = RenderSubPixel(r, t, samples, renderer, SubPixel.Center(in tr, in c));
            }

            if (!tlc)
            {
                ctl = RenderSubPixel(r, t, samples, renderer, SubPixel.Center(in tl, in c));
            }

            shared?.CloseLocalScope(samples);
            return (ctl + ctr + cbl + cbr) / 4f;
        }
    }
}