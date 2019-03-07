using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class RingPatternTests
    {
        [Fact]
        public void RingsExtendInXAndZ()
        {
            var pattern = new RingPattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 1)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0.708f, 0, 0.708f)).Should().Be(Colors.Black);
        }
    }
}