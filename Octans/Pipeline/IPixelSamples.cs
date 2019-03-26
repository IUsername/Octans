namespace Octans.Pipeline
{
    public interface IPixelSamples
    {
        Color GetOrAdd(in SubPixel sp, IPixelRenderer renderer);
        void Reset();
    }
}