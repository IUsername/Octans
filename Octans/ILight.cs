namespace Octans
{
    public interface ILight
    {
        Point Position { get; }
        Color Intensity { get; }
        Point[] SamplePoints { get; }

        int Samples { get; }
    }
}