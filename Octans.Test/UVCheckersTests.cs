using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class UVCheckersTests
    {
        [Fact]
        public void ReturnsAlternatingColorsAcrossUV()
        {
            var c = new UVCheckers(2, 2, Colors.Black, Colors.White);
            c.PatternAt(0f, 0f).Should().Be(Colors.Black);
            c.PatternAt(0.5f, 0f).Should().Be(Colors.White);
            c.PatternAt(0.0f, 0.5f).Should().Be(Colors.White);
            c.PatternAt(0.5f, 0.5f).Should().Be(Colors.Black);
            c.PatternAt(1.0f, 1.0f).Should().Be(Colors.Black);
        }
    }
}