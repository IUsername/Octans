namespace Octans.Reflection.Microfacet
{
    public interface IMicrofacetDistribution
    {
        float D(in Vector wh);
        float Lambda(in Vector w);
        float G1(in Vector w);
        float G(in Vector wo, in Vector wi);
        Vector SampleWh(in Vector wo, in Point2D u);
        float Pdf(in Vector wo, in Vector wh);
    }
}