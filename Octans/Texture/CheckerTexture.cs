using System;

namespace Octans.Texture
{
    public class CheckerTexture : TextureBase
    {
        public CheckerTexture(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        public override Color LocalColorAt(in Point localPoint) =>
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            (System.MathF.Floor(localPoint.X) + System.MathF.Floor(localPoint.Y) + System.MathF.Floor(localPoint.Z)) % 2f == 0f ? A : B;
    }
}