using FluentAssertions;
using Octans.Shading;
using Xunit;

namespace Octans.Test.Shading
{
    public class GGXNormalDistributionTests
    {
        [Fact]
        public void CenterNormalIsStraightUpWhenRoughnessIsZero()
        {
            var v = new Vector(1, 1, 1).Normalize();
            var n = GGXNormalDistribution.GGXVndf(v, 0f, 0f, 0, 0);
            n.Should().Be(new Vector(0, 0, 1));

            v = new Vector(1, 2, 3).Normalize();
            n = GGXNormalDistribution.GGXVndf(v, 0f, 0f, 0, 0);
            n.Should().Be(new Vector(0, 0, 1));
        }

        [Fact]
        public void ShouldHaveLowOffsetWhenNearZero()
        {
            var v = new Vector(0, 1, 1).Normalize();
            var n = GGXNormalDistribution.GGXVndf(v, 0.001f, 0.001f, 0f, 0f);
            n.Should().Be(new Vector(0, 0, 1));

            v = new Vector(1, 2, 1).Normalize();
            n = GGXNormalDistribution.GGXVndf(v, 0.001f, 0.001f, 0f, 0f);
            n.Should().Be(new Vector(0, 0, 1));
        }

        [Fact]
        public void RotatesNormalProportionalToAlphaXY()
        {
            var v = new Vector(1, 1, 1).Normalize();
            var n = GGXNormalDistribution.GGXVndf(v, 0f, 0f, 0, 0);
            n.Should().Be(new Vector(0, 0, 1));

            v = new Vector(1, 1, 1).Normalize();
            n = GGXNormalDistribution.GGXVndf(v, 1f, 0f, 0, 0);
            n.X.Should().BeGreaterThan(0f);
            n.Y.Should().Be(0f);

            v = new Vector(1, 1, 1).Normalize();
            n = GGXNormalDistribution.GGXVndf(v, 0f, 1f, 0, 0);
            n.Y.Should().BeGreaterThan(0f);
            n.X.Should().Be(0f);
        }
    }
}