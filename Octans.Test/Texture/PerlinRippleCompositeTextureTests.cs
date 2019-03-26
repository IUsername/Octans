using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class PerlinRippleCompositeTextureTests
    {
        [Fact]
        public void ContainsBasePattern()
        {
            var stripe = new StripeTexture(Colors.White, Colors.Black);
            var pattern = new PerlinRippleCompositeTexture(stripe, 0.5f);
            pattern.BaseTexture.Should().Be(stripe);
        }

        [Fact]
        public void DistortsBasePattern()
        {
            var stripe = new StripeTexture(Colors.White, Colors.Black);
            var pattern = new PerlinRippleCompositeTexture(stripe, 2f);
            stripe.LocalColorAt(new Point(-1.1f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(-1.1f, 0, 0)).Should().NotBe(Colors.White);
        }
    }
}