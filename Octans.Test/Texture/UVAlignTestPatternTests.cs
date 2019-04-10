using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
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
            c.ColorAt(new UVPoint(0.5f, 0.5f)).Should().Be(main);
            c.ColorAt(new UVPoint(0.1f, 0.9f)).Should().Be(ul);
            c.ColorAt(new UVPoint(0.9f, 0.9f)).Should().Be(ur);
            c.ColorAt(new UVPoint(0.1f, 0.1f)).Should().Be(bl);
            c.ColorAt(new UVPoint(0.9f, 0.1f)).Should().Be(br);
        }
    }
}