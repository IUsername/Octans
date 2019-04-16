namespace Octans.Reflection.Microfacet
{
    public abstract class MicrofacetDistributionBase : IMicrofacetDistribution
    {
        protected MicrofacetDistributionBase(bool sampleVisibleArea)
        {
            SampleVisibleArea = sampleVisibleArea;
        }

        protected bool SampleVisibleArea { get; }

        public abstract float D(in Vector wh);

        public abstract float Lambda(in Vector w);

        public float G1(in Vector w) => 1f / (1f + Lambda(in w));

        public float G(in Vector wo, in Vector wi) => 1f / (1f + Lambda(in wo) + Lambda(in wi));

        public abstract Vector SampleWh(in Vector wo, in Vector wi);

        public abstract float Pdf(in Vector wo, in Vector wh);
    }
}