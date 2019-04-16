using System;

namespace Octans
{
    public interface ISinkRgb
    {
        void Write(in ReadOnlySpan<float> rgb, in PixelArea area, in PixelVector fullResolution);
    }
}