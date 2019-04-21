namespace Octans
{
    public interface ICamera
    {
        Film Film { get; }
        float GenerateRayDifferential(in CameraSample sample, out RayDifferential ray);
        float GenerateRay(in CameraSample sample, out Ray ray);
    }
}