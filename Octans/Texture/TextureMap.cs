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
            var uv = Map(in point);
            return Texture.ColorAt(uv);
        }


        public override Color LocalColorAt(in Point localPoint) => PatternAt(in localPoint);
    }
}