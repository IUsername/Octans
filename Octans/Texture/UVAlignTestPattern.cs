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

        public Color ColorAt(UVPoint uv)
        {
            if (uv.V > 0.8f)
            {
                if (uv.U < 0.2f) return Ul;
                if (uv.U > 0.8f) return Ur;
            }
            else if (uv.V < 0.2f)
            {
                if (uv.U < 0.2f) return Bl;
                if (uv.U > 0.8f) return Br;
            }

            return Main;
        }
    }
}