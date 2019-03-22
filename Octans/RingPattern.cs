using System;

namespace Octans
{
    public class RingPattern : PatternBase
    {
        public RingPattern(Color a, Color b)
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