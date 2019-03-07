using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class CheckerPatternTests
    {
        [Fact]
        public void RepeatsInX()
        {
            var pattern = new CheckerPattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInY()
        {
            var pattern = new CheckerPattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0.99f, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 1.01f, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0, 2.01f, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInZ()
        {
            var pattern = new CheckerPattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 0.99f)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 1.01f)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 2.01f)).Should().Be(Colors.White);
        }
    }
}