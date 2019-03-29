using System;
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
        public void CenterNormalIsStraightUpWhenRoughnessIsZero()
        {
            var v = new Vector(1, 1, 0).Normalize();
            var r = GGXNormalDistribution.GGXVndf(v, 0f, 0, 0);
            r.Should().Be(new Vector(0,1,0));

            var sn = new Vector(0,1,0);
            var bn = new Vector(2f,2f,2f).Normalize();
            var iv = new Vector(0,-1,0);

            var (b1, b2) = MathFunction.OrthonormalVectorsPosZ(in bn);

        }

        [Fact]
        public void ShouldHaveLowOffsetWhenNearZero()
        {
            var v = new Vector(-1, 1, 0).Normalize();
            var r = GGXNormalDistribution.GGXVndf(v, 0.001f, 0f, 0f);
            var vp = v.Reflect(r);
            vp = new Vector(vp.X, -vp.Y, vp.Z);
            vp.Should().Be(v);

            v = new Vector(-1, 1, 1).Normalize();
            r = GGXNormalDistribution.GGXVndf(v, 0.001f, 0f, 0f);
            vp = v.Reflect(r);
            vp = new Vector(vp.X, -vp.Y, vp.Z);
            vp.Should().Be(v);
        }
    }
}