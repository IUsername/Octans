using System;

namespace Octans
{
    public class CheckerCompositePattern : PatternBase
    {
        public CheckerCompositePattern(IPattern a, IPattern b)
        {
            A = a;
            B = b;
        }

        public IPattern A { get; }
        public IPattern B { get; }

        public override Color LocalColorAt(Point localPoint)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((MathF.Floor(localPoint.X) + MathF.Floor(localPoint.Y) + MathF.Floor(localPoint.Z)) % 2f == 0f)
            {
                var aLocal = A.Transform.Inverse() * localPoint;
                return A.LocalColorAt(aLocal);
            }

            var bLocal = B.Transform.Inverse() * localPoint;
            return B.LocalColorAt(bLocal);
        }
    }
}