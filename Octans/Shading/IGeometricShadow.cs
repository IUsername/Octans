namespace Octans.Shading
{
    public interface IGeometricShadow
    {
        float Factor(in ShadingInfo info);
    }
}