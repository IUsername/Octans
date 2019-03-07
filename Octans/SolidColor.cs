namespace Octans
{
    public class SolidColor : PatternBase
    {
        public Color Color { get; }

        public SolidColor(Color color)
        {
            Color = color;
        }

        public override Color LocalColorAt(Point localPoint) => Color;
    }
}