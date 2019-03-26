namespace Octans.Texture
{
    public class UVAlignTestPattern : ITextureSource
    {
        public Color Main { get; }
        public Color Ul { get; }
        public Color Ur { get; }
        public Color Bl { get; }
        public Color Br { get; }

        public UVAlignTestPattern(Color main, Color ul, Color ur, Color bl, Color br)
        {
            Main = main;
            Ul = ul;
            Ur = ur;
            Bl = bl;
            Br = br;
        }

        public Color ColorAt(float u, float v)
        {
            if (v > 0.8f)
            {
                if (u < 0.2f) return Ul;
                if (u > 0.8f) return Ur;
            }
            else if (v < 0.2f)
            {
                if (u < 0.2f) return Bl;
                if (u > 0.8f) return Br;
            }

            return Main;
        }
    }
}