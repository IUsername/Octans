namespace Octans
{
    public class PerlinNoisePattern : PatternBase
    {
        public PerlinNoisePattern(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            // Normalize from 0 to 1.
            var noise = (Perlin.Noise(localPoint.X, localPoint.Y, localPoint.Z) + 1f) / 2f;
            var distance = B - A;
            return A + distance * noise;
        }
    }
}