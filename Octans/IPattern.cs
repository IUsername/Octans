namespace Octans
{
    public interface IPattern
    {
        Matrix Transform { get; }
        Matrix TransformInverse();
        Color LocalColorAt(in Point localPoint);
        void SetTransform(Matrix transform);
    }
}