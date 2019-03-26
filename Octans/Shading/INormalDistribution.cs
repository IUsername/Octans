namespace Octans.Shading
{
    public interface INormalDistribution
    {
        float Factor(in ShadingInfo info);
    }
}