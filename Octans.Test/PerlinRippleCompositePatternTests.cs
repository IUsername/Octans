using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PerlinRippleCompositePatternTests
    {
        [Fact]
        public void ContainsBasePattern()
        {
            var stripe = new StripePattern(Colors.White, Colors.Black);
            var pattern = new PerlinRippleCompositePattern(stripe, 0.5f);
            pattern.BasePattern.Should().Be(stripe);
        }

        [Fact]
        public void DistortsBasePattern()
        {
            var stripe = new StripePattern(Colors.White, Colors.Black);
            var pattern = new PerlinRippleCompositePattern(stripe, 2f);
            stripe.LocalColorAt(new Point(-1.1f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(-1.1f, 0, 0)).Should().NotBe(Colors.White);
        }
    }
}