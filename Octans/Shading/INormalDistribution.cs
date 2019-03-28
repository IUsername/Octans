namespace Octans.Shading
{
    public interface INormalDistribution
    {
        float Factor(in ShadingInfo info);
        (Vector wi, Color reflectance) Sample(in ShadingInfo info, float e0, float e1);
    }
}