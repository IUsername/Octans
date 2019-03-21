using System;

namespace Octans
{
    public class UVImage : ITextureSource
    {
        private readonly Canvas _canvas;

        public UVImage(Canvas canvas)
        {
            _canvas = canvas;
        }


        public Color PatternAt(float u, float v)
        {
            v = 1f - v;
            var x = u * (_canvas.Width - 1);
            var y = v * (_canvas.Height - 1);
            return _canvas.PixelAt(Round(in x), Round(in y));
        }

        private static int Round(in float value) => (int) MathF.Round(value);
    }
}