using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class BlendedCompositePatternTests
    {
        [Fact]
        public void BlendsTwoPatterns()
        {
            var s1 = new StripePattern(Colors.White, Colors.Black);
            var s2 = new StripePattern(Colors.White, Colors.Black);
            s2.SetTransform(Transforms.RotateY(MathF.PI/2f));
            var pattern = new BlendedCompositePattern(s1,s2);
            pattern.LocalColorAt(new Point(1f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(2f, 0f, 0)).Should().Be(new Color(0.5f, 0.5f, 0.5f));
            pattern.LocalColorAt(new Point(3f, 0f, 0f)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(2f, 0f, 1f)).Should().Be(Colors.White);
        }
    }
}