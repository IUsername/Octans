using System;

namespace Octans.Texture
{
    public class CheckerCompositeTexture : TextureBase
    {
        public CheckerCompositeTexture(ITexture a, ITexture b)
        {
            A = a;
            B = b;
        }

        public ITexture A { get; }
        public ITexture B { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((MathF.Floor(localPoint.X) + MathF.Floor(localPoint.Y) + MathF.Floor(localPoint.Z)) % 2f == 0f)
            {
                var aLocal = A.TransformInverse() * localPoint;
                return A.LocalColorAt(in aLocal);
            }

            var bLocal = B.TransformInverse() * localPoint;
            return B.LocalColorAt(in bLocal);
        }
    }
}