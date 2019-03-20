using System;

namespace Octans
{
    public class UVCheckers : ITextureSource
    {
        public UVCheckers(int width, int height, Color a, Color b)
        {
            Width = width;
            Height = height;
            A = a;
            B = b;
        }

        public int Width { get; }
        public int Height { get; }
        public Color A { get; }
        public Color B { get; }

        public Color PatternAt(float u, float v)
        {
            var u2 = MathF.Floor(u * Width);
            var v2 = MathF.Floor(v * Height);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return (u2 + v2) % 2f == 0f ? A : B;
        }
    }
}