using System;

namespace Octans
{
    public interface ISinkRgb
    {
        void Write(in Span<float> rgb, in PixelArea area, in PixelVector fullResolution);
    }
}