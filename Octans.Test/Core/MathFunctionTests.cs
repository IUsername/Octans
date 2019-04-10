using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class MathFunctionTests
    {
        [Fact]
        public void DegreesToRadians()
        {
            MathFunction.Rad(1f).Should().BeApproximately(0.0174533f, 0.000001f);
        }

        [Fact]
        public void RadiansToDegrees()
        {
            MathFunction.Deg(1f).Should().BeApproximately(57.2958f, 0.0001f);
        }
    }
}