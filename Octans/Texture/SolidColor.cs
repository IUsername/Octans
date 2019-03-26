namespace Octans.Texture
{
    public class SolidColor : TextureBase
    {
        public SolidColor(Color color)
        {
            Color = color;
        }

        public Color Color { get; }

        public override Color LocalColorAt(in Point localPoint) => Color;

        public static SolidColor Create(float r, float g, float b)
        {
            // TODO: Flyweight pattern helpful here?
            return new SolidColor(new Color(r,g,b));
        }
    }
}