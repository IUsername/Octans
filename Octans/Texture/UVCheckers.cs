using System;

namespace Octans.Texture
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

        public Color ColorAt(UVPoint uv)
        {
            var u2 = System.MathF.Floor(uv.U * Width);
            var v2 = System.MathF.Floor(uv.V * Height);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return (u2 + v2) % 2f == 0f ? A : B;
        }
    }
}