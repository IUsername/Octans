namespace Octans.Reflection
{
    public interface IFresnel
    {
        Spectrum Evaluate(float cosI);
    }
}