using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class RingTextureTests
    {
        [Fact]
        public void RingsExtendInXAndZ()
        {
            var texture = new RingTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 1)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0.708f, 0, 0.708f)).Should().Be(Colors.Black);
        }
    }
}