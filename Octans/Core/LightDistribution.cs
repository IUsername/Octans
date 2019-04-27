using Octans.Sampling;

namespace Octans
{
    public enum LightSampleStrategy
    {
    }

    internal class LightDistribution
    {
        public Distribution1D Lookup(in Point p)
        {
            throw new System.NotImplementedException();
        }

        public static LightDistribution CreateLightSampleDistribution(LightSampleStrategy lightSampleStrategy, in IScene scene)
        {
            throw new System.NotImplementedException();
        }
    }
}
