using FluentAssertions;
using Octans.Geometry;
using Octans.Shading;
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

        [Fact]
        public void PatternWithObjectTransform()
        {
            var obj = new Sphere();
            obj.SetTransform(Transform.Scale(2, 2, 2));
            var texture = new TestTexture();
            texture.ShapeColor(obj, new Point(2f, 3f, 4f)).Should().Be(new Color(1f, 1.5f, 2f));
        }

        [Fact]
        public void PatternWithPatternTransform()
        {
            var obj = new Sphere();
            var texture = new TestTexture();
            texture.SetTransform(Transform.Scale(2, 2, 2));
            texture.ShapeColor(obj, new Point(2f, 3f, 4f)).Should().Be(new Color(1f, 1.5f, 2f));
        }

        [Fact]
        public void PatternWithObjectAndPatternTransform()
        {
            var obj = new Sphere();
            obj.SetTransform(Transform.Scale(2, 2, 2));
            var texture = new TestTexture();
            texture.SetTransform(Transform.Translate(0.5f, 1, 1.5f));
            texture.ShapeColor(obj, new Point(2.5f, 3f, 3.5f)).Should().Be(new Color(0.75f, 0.5f, 0.25f));
        }
    }
}