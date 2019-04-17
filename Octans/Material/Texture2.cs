namespace Octans.Material
{
    public interface ITexture2<out T>
    {
        T Evaluate(in SurfaceInteraction si);
    }

    public sealed class ConstantTexture<T> : ITexture2<T>
    {
        private readonly T _value;

        public ConstantTexture(in T value)
        {
            _value = value;
        }

        public T Evaluate(in SurfaceInteraction si) => _value;
    }
}