namespace Octans
{
    public class PointLight : ILight
    {
        public PointLight(Point position, Color intensity)
        {
            Position = position;
            Intensity = intensity;
        }

        public Point Position { get; }
        public Color Intensity { get; }
    }
}