namespace Octans.Texture
{
    public class PerlinRippleCompositeTexture : TextureBase
    {
        public PerlinRippleCompositeTexture(ITexture baseTexture, float perturbation)
        {
            BaseTexture = baseTexture;
            Perturbation = perturbation;
        }

        public ITexture BaseTexture { get; }
        public float Perturbation { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            var x = Perlin.Noise(localPoint.X, localPoint.Y, localPoint.Z);
            var y = Perlin.Noise(localPoint.Z, localPoint.X, localPoint.Y);
            var z = Perlin.Noise(localPoint.Y, localPoint.Z, localPoint.X);
            var offset = new Vector(x, y, z) * Perturbation;

            var global = Transform.Matrix * (localPoint + offset);
            var baseLocal = BaseTexture.Transform.Inverse * global;
            return BaseTexture.LocalColorAt(in baseLocal);
        }
    }
}