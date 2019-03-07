using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class StripePatternTests
    {
        [Fact]
        public void ContainsTwoColors()
        {
            var pattern = new StripePattern(Colors.White, Colors.Black);
            pattern.A.Should().Be(Colors.White);
            pattern.B.Should().Be(Colors.Black);
        }

        [Fact]
        public void PatternIsConstantInY()
        {
            var pattern = new StripePattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 1, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 2, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void PatternIsConstantInZ()
        {
            var pattern = new StripePattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 1)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 2)).Should().Be(Colors.White);
        }

        [Fact]
        public void PatternAlternatesInX()
        {
            var pattern = new StripePattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.9f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(-0.1f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(-1f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(-1.1f, 0, 0)).Should().Be(Colors.White);
        }
    }
}