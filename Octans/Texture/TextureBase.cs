namespace Octans.Texture
{
    public abstract class TextureBase : ITexture
    {
        protected TextureBase()
        {
            Transform = Transform.Identity;
        }

        public Transform Transform { get; protected set; }

        public abstract Color LocalColorAt(in Point localPoint);

        public void SetTransform(Transform transform)
        {
            Transform = transform;
        }
    }
}