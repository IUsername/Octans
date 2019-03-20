namespace Octans
{
    public class TextureMap : PatternBase
    {
        public ITextureSource Texture { get; }
        public PointToUV Map { get; }

        public TextureMap(ITextureSource texture, PointToUV map)
        {
            Texture = texture;
            Map = map;
        }

        public Color PatternAt(Point point)
        {
            var (u,v) = Map(point);
            return Texture.PatternAt(u, v);
        }


        public override Color LocalColorAt(Point localPoint)
        {
            return PatternAt(localPoint);
        }
    }
}