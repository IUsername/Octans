namespace Octans.Texture
{
    public class BlendedCompositeTexture : TextureBase
    {
        public BlendedCompositeTexture(ITexture a, ITexture b)
        {
            A = a;
            B = b;
        }

        public ITexture A { get; }
        public ITexture B { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            var aLocal = A.Transform ^ localPoint;
            var a = A.LocalColorAt(in aLocal);

            var bLocal = B.Transform ^ localPoint;
            var b = B.LocalColorAt(in bLocal);

            return (a + b) / 2f;
        }
    }
}