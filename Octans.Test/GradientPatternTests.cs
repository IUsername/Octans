using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class GradientPatternTests
    {
        [Fact]
        public void InterpolatesBetweenColors()
        {
            var pattern = new GradientPattern(Colors.White, Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.25f, 0, 0)).Should().Be(new Color(0.75f, 0.75f, 0.75f));
            pattern.LocalColorAt(new Point(0.5f, 0, 0)).Should().Be(new Color(0.5f, 0.5f, 0.5f));
            pattern.LocalColorAt(new Point(0.75f, 0, 0)).Should().Be(new Color(0.25f, 0.25f, 0.25f));
        }
    }
}