namespace Octans
{
    public interface IPattern
    {
        Matrix Transform { get; }
        Color LocalColorAt(Point localPoint);
        void SetTransform(Matrix transform);
    }
}