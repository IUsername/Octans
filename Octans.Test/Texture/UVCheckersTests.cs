using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class UVCheckersTests
    {
        [Fact]
        public void ReturnsAlternatingColorsAcrossUV()
        {
            var c = new UVCheckers(2, 2, Colors.Black, Colors.White);
            c.ColorAt(0f, 0f).Should().Be(Colors.Black);
            c.ColorAt(0.5f, 0f).Should().Be(Colors.White);
            c.ColorAt(0.0f, 0.5f).Should().Be(Colors.White);
            c.ColorAt(0.5f, 0.5f).Should().Be(Colors.Black);
            c.ColorAt(1.0f, 1.0f).Should().Be(Colors.Black);
        }
    }
}