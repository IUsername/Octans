namespace Octans.Reflection
{
    public class FresnelNoOp : IFresnel
    {
        public Spectrum Evaluate(float cosI)
        {
            return new Spectrum(1f);
        }
    }
}   