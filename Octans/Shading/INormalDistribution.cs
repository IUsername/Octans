namespace Octans.Shading
{
    public interface INormalDistribution
    {
        float Factor(in ShadingInfo info);
        (Vector wi, Color reflectance) Sample(in IntersectionInfo info, in LocalFrame localFrame, float e0, float e1);

        (Vector wi, Color transmissionFactor) SampleTransmission(in IntersectionInfo info,
                                                                                 in LocalFrame localFrame,
                                                                                 float e0,
                                                                                 float e1);
    }
}