using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class StripeTextureTests
    {
        [Fact]
        public void ContainsTwoColors()
        {
            var texture = new StripeTexture(Colors.White, Colors.Black);
            texture.A.Should().Be(Colors.White);
            texture.B.Should().Be(Colors.Black);
        }

        [Fact]
        public void PatternIsConstantInY()
        {
            var texture = new StripeTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 1, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 2, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void PatternIsConstantInZ()
        {
            var texture = new StripeTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 1)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 2)).Should().Be(Colors.White);
        }

        [Fact]
        public void PatternAlternatesInX()
        {
            var texture = new StripeTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.9f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(-0.1f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(-1f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(-1.1f, 0, 0)).Should().Be(Colors.White);
        }
    }
}