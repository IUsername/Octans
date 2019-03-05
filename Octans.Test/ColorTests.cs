using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ColorTests
    {
        [Fact]
        public void ColorIsRgbTuple()
        {
            var c = Color.Create(-0.5, 0.4, 1.7);
            c.Red.Should().Be(-0.5);
            c.Green.Should().Be(0.4);
            c.Blue.Should().Be(1.7);
        }

        [Fact]
        public void AddingColors()
        {
            var c1 = Color.Create(0.9, 0.6, 0.75);
            var c2 = Color.Create(0.7, 0.1, 0.25);
            (c1+c2).Should().BeEquivalentTo(Color.Create(1.6,0.7,1.0));
        }

        [Fact]
        public void SubtractingColors()
        {
            var c1 = Color.Create(0.9, 0.6, 0.75);
            var c2 = Color.Create(0.7, 0.1, 0.25);
            (c1 - c2).Should().BeEquivalentTo(Color.Create(0.2, 0.5, 0.5));
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var c = Color.Create(0.2, 0.3, 0.4);
            (c * 2).Should().BeEquivalentTo(Color.Create(0.4, 0.6, 0.8));
        }

        [Fact]
        public void MultiplyingColors()
        {
            var c1 = Color.Create(1.0, 0.2, 0.4);
            var c2 = Color.Create(0.9, 1.0, 0.1);
            (c1 * c2).Should().BeEquivalentTo(Color.Create(0.9, 0.2, 0.04));
        }
    }
}