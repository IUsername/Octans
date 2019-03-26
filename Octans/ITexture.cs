namespace Octans
{
    public interface ITexture
    {
        Matrix Transform { get; }
        Matrix TransformInverse();
        Color LocalColorAt(in Point localPoint);
        void SetTransform(Matrix transform);
    }
}