namespace Octans
{
    public interface IPixelRenderer
    {
        Color Render(in SubPixel sp);
    }
}