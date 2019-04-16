using System;

namespace Octans.Texture
{
    public class GradientTexture : TextureBase
    {
        public GradientTexture(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            var distance = B - A;
            var fraction = localPoint.X - System.MathF.Floor(localPoint.X);
            return A + distance * fraction;
        }
    }
}