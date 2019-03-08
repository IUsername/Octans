using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PerlinNoisePatternTests
    {
        [Fact]
        public void ContainsTwoColors()
        {
            var pattern = new PerlinNoisePattern(Colors.White, Colors.Black);
            pattern.A.Should().Be(Colors.White);
            pattern.B.Should().Be(Colors.Black);
        }

        [Fact]
        public void MixesTwoColors()
        {
            var pattern = new PerlinNoisePattern(Colors.White, Colors.Black);
            var c1 = pattern.LocalColorAt(new Point(0, 0, 0));
            var c2 = pattern.LocalColorAt(new Point(0.2f, 0.3f, 0.4f));
            var c3 = pattern.LocalColorAt(new Point(0.2f, 0.3f, 0.5f));
            CheckForValidGray(c1);
            CheckForValidGray(c2);
            CheckForValidGray(c3);
            c1.Should().NotBe(c2);
            c1.Should().NotBe(c3);
        }

        private static void CheckForValidGray(Color color)
        {
            color.Red.Should().BeApproximately(color.Green, 0.0001f);
            color.Green.Should().BeApproximately(color.Blue, 0.0001f);
            color.Red.Should().BeInRange(0f, 1f);
        }
    }
}