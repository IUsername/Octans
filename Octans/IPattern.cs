namespace Octans
{
    public interface IPattern
    {
        Matrix Transform { get; }
        Matrix TransformInverse();
        Color LocalColorAt(Point localPoint);
        void SetTransform(Matrix transform);
    }
}