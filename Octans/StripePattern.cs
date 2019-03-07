using System;

namespace Octans
{
    public class StripePattern : PatternBase
    {
        public StripePattern(Color a, Color b)
        {
            A = a;
            B = b;
        }

        public Color A { get; }
        public Color B { get; }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public override Color LocalColorAt(Point localPoint) => MathF.Floor(localPoint.X) % 2f == 0f ? A : B;
    }
}