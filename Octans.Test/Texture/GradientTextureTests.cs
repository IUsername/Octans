using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class GradientTextureTests
    {
        [Fact]
        public void InterpolatesBetweenColors()
        {
            var texture = new GradientTexture(Colors.White, Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.25f, 0, 0)).Should().Be(new Color(0.75f, 0.75f, 0.75f));
            texture.LocalColorAt(new Point(0.5f, 0, 0)).Should().Be(new Color(0.5f, 0.5f, 0.5f));
            texture.LocalColorAt(new Point(0.75f, 0, 0)).Should().Be(new Color(0.25f, 0.25f, 0.25f));
        }
    }
}