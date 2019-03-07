using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SolidColorTests
    {
        [Fact]
        public void SameColorEverywhere()
        {
            var color = new Color(0, 1, 0);
            var pattern = new SolidColor(color);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(color);
            pattern.LocalColorAt(new Point(0, 2, 0)).Should().Be(color);
            pattern.LocalColorAt(new Point(0, 0, 9)).Should().Be(color);
        }
    }
}