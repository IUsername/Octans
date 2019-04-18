namespace Octans
{
    public interface ILight
    {
        Point Position { get; }
        Color Intensity { get; }
        Point[] SamplePoints { get; }

        int Samples { get; }
        void Preprocess(IScene scene);

        LightType Type { get; }
        Spectrum Le(in Ray ray);
    }

    public enum LightType
    {
        Unknown = 0,
        DeltaPosition = 1,
        DeltaDirection = 2,
        Area = 4,
        Infinite = 8
    }
}