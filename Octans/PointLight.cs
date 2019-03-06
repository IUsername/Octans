namespace Octans
{
    public class PointLight
    {
        public PointLight(Tuple position, Color intensity)
        {
            Position = position;
            Intensity = intensity;
        }

        public Tuple Position { get; }
        public Color Intensity { get; }
    }
}