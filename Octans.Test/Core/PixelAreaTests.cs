using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PixelAreaTests
    {
        [Fact]
        public void HoldMinAndMax()
        {
            var pa = new PixelArea(new PixelCoordinate(10, 20), new PixelCoordinate(50, 100));
            pa.Min.Should().Be(new PixelCoordinate(10, 20));
            pa.Max.Should().Be(new PixelCoordinate(50, 100));
        }

        [Fact]
        public void DetermineArea()
        {
            var pa = new PixelArea(new PixelCoordinate(10, 20), new PixelCoordinate(50, 100));
            pa.Area().Should().Be(80 * 40);
        }

        [Fact]
        public void FindIntersect()
        {
            var a = new PixelArea(new PixelCoordinate(0, 0), new PixelCoordinate(50, 100));
            var b = new PixelArea(new PixelCoordinate(20, 30), new PixelCoordinate(100, 60));
            var i = PixelArea.Intersect(a, b);
            i.Min.X.Should().Be(20);
            i.Min.Y.Should().Be(30);
            i.Max.X.Should().Be(50);
            i.Max.Y.Should().Be(60);
        }
    }
}