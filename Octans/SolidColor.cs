namespace Octans
{
    public class SolidColor : PatternBase
    {
        public SolidColor(Color color)
        {
            Color = color;
        }

        public Color Color { get; }

        public override Color LocalColorAt(in Point localPoint) => Color;
    }
}