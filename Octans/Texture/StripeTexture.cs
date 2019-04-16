using System;

namespace Octans.Texture
{
    public class StripeTexture : TextureBase
    {
        public StripeTexture(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public override Color LocalColorAt(in Point localPoint) => System.MathF.Floor(localPoint.X) % 2f == 0f ? A : B;
    }
}