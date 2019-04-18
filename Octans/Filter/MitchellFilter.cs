using System.Numerics;
using static System.MathF;

namespace Octans.Filter
{
    /// <summary>
    ///     Mitchell, D. P., and A. N. Netravali. 1988.
    ///     Reconstruction filters in computer graphics.
    ///     Computer Graphics (SIGGRAPH ’88 Proceedings), Volume 22, 221–28.
    /// </summary>
    public sealed class MitchellFilter : FilterBase
    {
        private readonly float _b;
        private readonly float _c;

        /// <summary>
        ///     Creates a Mitchel & Netravalli filter. This filter has a good trade-off between
        ///     ringing and blurring. It has negative lobes near the radius extent.
        /// </summary>
        /// <param name="radius">Radius of the filter in x and y.</param>
        /// <param name="b">B component. Should follow B + 2C = 1</param>
        /// <param name="c">C component. Should follow B + 2C = 1</param>
        public MitchellFilter(in Vector2 radius, float b, float c) : base(in radius)
        {
            _b = b;
            _c = c;
        }

        public override float Evaluate(in Point2D p) => Eval1D(p.X * InverseRadius.X) * Eval1D(p.Y * InverseRadius.Y);

        private float Eval1D(float x)
        {
            x = Abs(2f * x);
            if (x > 1f)
            {
                return ((-_b - 6f * _c) * x * x * x +
                        (6f * _b + 30 * _c) * x * x +
                        (-12f * _b - 48 * _c) * x +
                        (8f * _b + 24f * _c)) * (1f / 6f);
            }

            return ((12f - 9f * _b - 6f * _c) * x * x * x +
                    (-18f + 12f * _b + 6f * _c) * x * x +
                    (6f - 2f * _b)) * (1f / 6f);
        }
    }
}