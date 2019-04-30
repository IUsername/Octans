namespace Octans
{
    public interface IAreaLight : ILight
    {
        Spectrum L(Interaction intr, in Vector w);
    }
}