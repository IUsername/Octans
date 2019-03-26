namespace Octans.Texture
{
    public class TextureMap : TextureBase
    {
        public TextureMap(ITextureSource texture, PointToUV map)
        {
            Texture = texture;
            Map = map;
        }

        public ITextureSource Texture { get; }
        public PointToUV Map { get; }

        public Color PatternAt(in Point point)
        {
            var (u, v) = Map(in point);
            return Texture.ColorAt(u, v);
        }


        public override Color LocalColorAt(in Point localPoint) => PatternAt(in localPoint);
    }
}