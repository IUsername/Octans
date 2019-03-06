namespace Octans
{
    public class Canvas
    {
        private readonly Color[,] _data;

        public Canvas(int width, int height)
        {
            Width = width;
            Height = height;
            _data = new Color[width, height];
        }

        public int Width { get; }

        public int Height { get; }

        public void WritePixel(Color c, int x, int y)
        {
            _data[x, y] = c;
        }

        public Color PixelAt(int x, int y) => _data[x, y];

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

        public bool IsInBounds(int x, int y)
        {
            if (x < 0 || x > Width)
            {
                return false;
            }

            if (y < 0 || y > Height)
            {
                return false;
            }

            return true;
        }
    }
}