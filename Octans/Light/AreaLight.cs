namespace Octans.Light
{
    public abstract class AreaLight : Light, IAreaLight
    {
        protected AreaLight(Transform lightToWorld, IMedium mediumInterface, int nSamples = 1) : base(
            LightType.Area, lightToWorld, mediumInterface, nSamples)
        {
        }

        public abstract Spectrum L(Interaction intr, in Vector w);
    }
}