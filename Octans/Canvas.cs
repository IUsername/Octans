using System;

namespace Octans
{
    public class Canvas
    {
        public int Width { get; }

        public int Height { get; }

        private readonly Color[,] _data;

        public Canvas(int width, int height)
        {
            Width = width;
            Height = height;
            _data = new Color[width,height];
        }

        public void WritePixel(Color c, int x, int y)
        {
            _data[x, y] = c;
        }

        public Color PixelAt(int x, int y)
        {
            return _data[x, y];
        }

        public void SetAllPixels(Color c)
        {
            for (var j = 0; j < Height; j++)
            {
                for (var i = 0; i < Width; i++)
                {
                    _data[i, j] = c;
                }
            }
        }
    }
}