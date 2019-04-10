using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class UVPointTests
    {
        [Fact]
        public void TakesUV()
        {
            var uv = new UVPoint(0,1);
            uv.U.Should().Be(0f);
            uv.V.Should().Be(1f);
        }

        [Fact]
        public void Scale()
        {
            var uv = new UVPoint(0, 1);
            (uv * 10f).Should().Be(new UVPoint(0, 10));
        }
    }
}