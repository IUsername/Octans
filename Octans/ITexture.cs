namespace Octans
{
    public interface ITexture
    {
        Transform Transform { get; }
        Color LocalColorAt(in Point localPoint);
        void SetTransform(Transform transform);
    }
}