namespace Octans
{
    public interface ILight
    {
        Point Position { get; }
        Color Intensity { get; }
    }
}