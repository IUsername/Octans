using Octans.Texture;

namespace Octans.Test.Texture
{
    internal class TestTexture : TextureBase
    {
        public override Color LocalColorAt(in Point localPoint) => new Color(localPoint.X, localPoint.Y, localPoint.Z);
    }
}