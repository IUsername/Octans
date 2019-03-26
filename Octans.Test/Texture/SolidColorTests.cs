using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class SolidColorTests
    {
        [Fact]
        public void SameColorEverywhere()
        {
            var color = new Color(0, 1, 0);
            var solidColor = new SolidColor(color);
            solidColor.LocalColorAt(new Point(0, 0, 0)).Should().Be(color);
            solidColor.LocalColorAt(new Point(0, 2, 0)).Should().Be(color);
            solidColor.LocalColorAt(new Point(0, 0, 9)).Should().Be(color);
        }

        [Fact]
        public void CreateMethod()
        {
            var color = new Color(0, 1, 0);
            var sc1 = new SolidColor(color);
            var sc2 = SolidColor.Create(0, 1, 0);
            sc1.Color.Should().Be(sc2.Color);
        }
    }
}