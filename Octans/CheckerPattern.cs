using System;

namespace Octans
{
    public class CheckerPattern : PatternBase
    {
        public CheckerPattern(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        public override Color LocalColorAt(Point localPoint) =>
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            (MathF.Floor(localPoint.X) + MathF.Floor(localPoint.Y) + MathF.Floor(localPoint.Z)) % 2f == 0f ? A : B;
    }
}