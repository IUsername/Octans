using System;
using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class BlendedCompositeTextureTests
    {
        [Fact]
        public void BlendsTwoTextures()
        {
            var s1 = new StripeTexture(Colors.White, Colors.Black);
            var s2 = new StripeTexture(Colors.White, Colors.Black);
            s2.SetTransform(Transform.RotateY(System.MathF.PI / 2f));
            var texture = new BlendedCompositeTexture(s1, s2);
            texture.LocalColorAt(new Point(1f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(2f, 0f, 0)).Should().Be(new Color(0.5f, 0.5f, 0.5f));
            texture.LocalColorAt(new Point(3f, 0f, 0f)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(2f, 0f, 1f)).Should().Be(Colors.White);
        }
    }
}