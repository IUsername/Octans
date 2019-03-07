using System;

namespace Octans
{
    public readonly struct StripePattern
    {
        public StripePattern(Color a, Color b) : this(a, b, Matrix.Identity)
        {
        }

        public StripePattern(Color a, Color b, Matrix transform)
        {
            A = a;
            B = b;
            Transform = transform;
        }

        public Color A { get; }
        public Color B { get; }
        public Matrix Transform { get; }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public Color ColorAt(Point point) => MathF.Floor(point.X) % 2f == 0f ? A : B;

        public Color ColorAt(Point worldPoint, IShape shape)
        {
            var localPoint = worldPoint.ToLocal(shape);
            var patternPoint = Transform.Inverse() * localPoint;
            return ColorAt(patternPoint);
        }
    }
}