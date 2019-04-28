using FluentAssertions;
using Octans.Geometry;
using Xunit;

namespace Octans.Test.Texture
{
    public class TextureBaseTests
    {
        [Fact]
        public void DefaultTransform()
        {
            var texture = new TestTexture();
            texture.Transform.Should().Be(Transform.Identity);
        }

        [Fact]
        public void CanSetTransform()
        {
            var transform = Transform.Translate(1, 2, 3);
            var texture = new TestTexture();
            texture.SetTransform(transform);
            texture.Transform.Should().Be(transform);
        }

     
    }
}