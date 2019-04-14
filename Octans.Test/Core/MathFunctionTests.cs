using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class MathFunctionTests
    {
        [Fact]
        public void Lerp()
        {
            MathFunction.Lerp(0, 10, 0.5f).Should().Be(5f);
            MathFunction.Lerp(-10, 10, 0.5f).Should().Be(0f);
            MathFunction.Lerp(-100, -10, 0.5f).Should().Be(-55f);
            MathFunction.Lerp(0, 100, 0.0f).Should().Be(0f);
            MathFunction.Lerp(0, 100, 1.0f).Should().Be(100f);
        }

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

        [Fact]
        public void OrthonormalPosZ()
        {
            var n = new Normal(1f,1f,1f).Normalize();
            var (v1, v2) = MathFunction.OrthonormalPosZ(in n);
            var v3 = (Vector) n;
            v1.Magnitude().Should().BeApproximately(1f, 0.0001f);
            v2.Magnitude().Should().BeApproximately(1f, 0.0001f);
            v3.Magnitude().Should().BeApproximately(1f, 0.0001f);
            (v1 % v2).Should().BeApproximately(0.0f, 0.0001f);
            (v2 % v3).Should().BeApproximately(0.0f, 0.0001f);
            (v1 % v3).Should().BeApproximately(0.0f, 0.0001f);
        }

        [Fact]
        public void OneMinusEpsilon()
        {
            var v = MathFunction.OneMinusEpsilon;
            v.Should().Be(0.99999994F);
        }
    }
}