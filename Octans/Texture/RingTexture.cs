using System;

namespace Octans.Texture
{
    public class RingTexture : TextureBase
    {
        public RingTexture(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        public override Color LocalColorAt(in Point localPoint) =>
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            MathF.Floor(MathF.Sqrt(localPoint.X * localPoint.X + localPoint.Z * localPoint.Z)) % 2f == 0f ? A : B;
    }
}