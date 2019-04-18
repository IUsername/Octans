namespace Octans.Reflection
{
    public interface IBxDFCollection
    {
        IBxDF this[int index] { get; }
        void Set(in IBxDF bxdf, int index);
    }
}