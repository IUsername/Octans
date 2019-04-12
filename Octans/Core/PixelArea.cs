namespace Octans
{
    public readonly struct PixelArea
    {
        public PixelCoordinate Min { get; }
        public PixelCoordinate Max { get; }

        public PixelArea(PixelCoordinate min, PixelCoordinate max)
        {
            Min = min;
            Max = max;
        }
    }
}