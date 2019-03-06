using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ColorTests
    {
        [Fact]
        public void ColorIsRgbTuple()
        {
            var c = Color.RGB(-0.5f, 0.4f, 1.7f);
            c.Red.Should().Be(-0.5f);
            c.Green.Should().Be(0.4f);
            c.Blue.Should().Be(1.7f);
        }

        [Fact]
        public void AddingColors()
        {
            var c1 = Color.RGB(0.9f, 0.6f, 0.75f);
            var c2 = Color.RGB(0.7f, 0.1f, 0.25f);
            (c1 + c2 == Color.RGB(1.6f, 0.7f, 1.0f)).Should().BeTrue();
        }

        [Fact]
        public void SubtractingColors()
        {
            var c1 = Color.RGB(0.9f, 0.6f, 0.75f);
            var c2 = Color.RGB(0.7f, 0.1f, 0.25f);
            (c1 - c2 == Color.RGB(0.2f, 0.5f, 0.5f)).Should().BeTrue();
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var c = Color.RGB(0.2f, 0.3f, 0.4f);
            (c * 2 == Color.RGB(0.4f, 0.6f, 0.8f)).Should().BeTrue();
        }

        [Fact]
        public void MultiplyingColors()
        {
            var c1 = Color.RGB(1.0f, 0.2f, 0.4f);
            var c2 = Color.RGB(0.9f, 1.0f, 0.1f);
            (c1 * c2 == Color.RGB(0.9f, 0.2f, 0.04f)).Should().BeTrue();
        }
    }
}