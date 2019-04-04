namespace Octans.Pipeline
{
    public sealed class SamplesPerPixelRenderer : IPixelRenderer
    {
        private readonly IPixelRenderer _renderer;


        public SamplesPerPixelRenderer(int samplesPerPixel, IPixelRenderer renderer)
        {
            SamplesPerPixel = samplesPerPixel;
            _renderer = renderer;
        }

        public int SamplesPerPixel { get; }


        public Color Render(in SubPixel sp) => SamplePixel(SamplesPerPixel, _renderer, in sp);

        private static Color SamplePixel(int samplesPerPixel, IPixelRenderer renderer, in SubPixel sp)
        {
            var total = Colors.Black;
            for (var i = 0; i < samplesPerPixel; i++)
            {
                var (u, v) = QuasiRandom.Next(i);
                var uS = (int) (u * 1000000);
                var vS = (int) (v * 1000000);
                var spS = new SubPixel(sp.X, sp.Y, 1000000, uS, vS);
                total += renderer.Render(in spS);
            }

            return total / samplesPerPixel;
        }
    }
}