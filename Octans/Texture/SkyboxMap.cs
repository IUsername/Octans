namespace Octans.Texture
{
    public class SkyBoxMap : TextureBase
    {
        public ITextureSource SkyBoxImage { get; }

        public SkyBoxMap(ITextureSource skyBoxImage)
        {
            SkyBoxImage = skyBoxImage;
        }

        public override Color LocalColorAt(in Point localPoint)
        {
            var uv = UVMapping.SkyBox(in localPoint);
            return SkyBoxImage.ColorAt(uv);
        }
    }
}