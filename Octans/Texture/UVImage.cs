using System;

namespace Octans.Texture
{
    public class UVImage : ITextureSource
    {
        private readonly Canvas _canvas;

        public UVImage(Canvas canvas)
        {
            _canvas = canvas;
        }


        public Color ColorAt(UVPoint uv)
        {
            var v = 1f - uv.V;
            var x = uv.U * (_canvas.Width - 1);
            var y = v * (_canvas.Height - 1);
            return _canvas.PixelAt(Round(in x), Round(in y));
        }

        private static int Round(in float value) => (int) System.MathF.Round(value);
    }
}