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
            return SolidColor.Create(new Color(r,g,b));
        }

        public static SolidColor Create(in Color color)
        {
            return new SolidColor(color);
        }
    }
}