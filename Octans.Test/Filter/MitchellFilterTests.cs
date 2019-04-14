using System;
using System.Numerics;
using FluentAssertions;
using Octans.Filter;
using Xunit;

namespace Octans.Test.Filter
{
    public class MitchellFilterTests
    {
        [Fact]
        public void CapturesRadius()
        {
            var f = new MitchellFilter(new Vector2(2f), 0.5f, 0.25f);
            f.Radius.Should().Be(new Vector2(2f));
            f.InverseRadius.Should().Be(new Vector2(0.5f));
        }

        [Fact]
        public void ValuesAtRadiusShouldBeZero()
        {
            var f = new MitchellFilter(new Vector2(2f), 0.5f, 0.25f);
            f.Evaluate(new Point2D(2f, 0f)).Should().BeApproximately(0f, 0.001f);
            f.Evaluate(new Point2D(0f, 2f)).Should().BeApproximately(0f, 0.001f);
            f.Evaluate(new Point2D(0f, -2f)).Should().BeApproximately(0f, 0.001f);
            f.Evaluate(new Point2D(MathF.Sqrt(2f), MathF.Sqrt(2f))).Should().BeApproximately(0f, 0.001f);
        }

        [Fact]
        public void ValueAtOrigin()
        {
            var b = 0.5f;
            var zero1D = (6f - 2 * b) * (1f / 6f);
            var f = new MitchellFilter(new Vector2(2f), b, 0.25f);
            f.Evaluate(new Point2D(0f, 0f)).Should().BeApproximately(zero1D * zero1D, 0.0001f);
        }

        [Fact]
        public void NegativeLobesNearRadiusExtent()
        {
            var f = new MitchellFilter(new Vector2(2f), 0.5f, 0.25f);
            f.Evaluate(new Point2D(1.8f, 0f)).Should().BeLessThan(0f);
        }
    }
}