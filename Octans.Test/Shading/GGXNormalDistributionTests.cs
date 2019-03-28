using FluentAssertions;
using Octans.Geometry;
using Octans.Light;
using Octans.Shading;
using Xunit;

namespace Octans.Test.Shading
{
    public class GGXNormalDistributionTests
    {
        [Fact]
        public void PerfectReflectionWhenRoughnessIsZero()
        {
            var ggx = GGXNormalDistribution.Instance;

            var r = GGXNormalDistribution.GGXVndf(new Vector(0, 1, 0), 1f, 0, 0);
            r.Should().Be(new Vector(0, 1, 0));
            r = GGXNormalDistribution.GGXVndf(new Vector(0, 1, 0), 0.5f, 0, 0);
            r.Should().Be(new Vector(0, 1, 0));
            var v = new Vector(1, 1, 0).Normalize();
            r = GGXNormalDistribution.GGXVndf(v, 0f, 0, 0);
            var wi = v.Reflect(r);
            r.Should().Be(v);
           
        }
    }
}