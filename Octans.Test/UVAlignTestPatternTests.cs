using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class UVAlignTestPatternTests
    {
        [Fact]
        public void DifferentColorInCorners()
        {
            var main = Colors.White;
            var ul = Colors.Red;
            var ur = Colors.Yellow;
            var bl = Colors.Green;
            var br = Colors.Blue;

            var c = new UVAlignTestPattern(main, ul, ur, bl, br);
            c.PatternAt(0.5f, 0.5f).Should().Be(main);
            c.PatternAt(0.1f, 0.9f).Should().Be(ul);
            c.PatternAt(0.9f, 0.9f).Should().Be(ur);
            c.PatternAt(0.1f, 0.1f).Should().Be(bl);
            c.PatternAt(0.9f, 0.1f).Should().Be(br);
        }
    }
}