using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class CheckerTextureTests
    {
        [Fact]
        public void RepeatsInX()
        {
            var texture = new CheckerTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInY()
        {
            var texture = new CheckerTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0.99f, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 1.01f, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0, 2.01f, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInZ()
        {
            var texture = new CheckerTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 0.99f)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 1.01f)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 2.01f)).Should().Be(Colors.White);
        }
    }
}