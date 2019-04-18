using Octans.Sampling;

namespace Octans
{
    public interface ICamera
    {
        Film Film { get; }
        float GenerateRayDifferential(CameraSample cameraSample, out Ray ray);
    }
}