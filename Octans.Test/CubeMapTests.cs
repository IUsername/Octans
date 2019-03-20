using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class CubeMapTests
    {
        [Fact]
        public void ShouldAlignColorsAtCorners()
        {
            var orange = new Color(1f, 0.5f, 0f);
            var left = new UVAlignTestPattern(Colors.Yellow, Colors.Cyan, Colors.Red, Colors.Blue, orange);
            var front = new UVAlignTestPattern(Colors.Cyan, Colors.Red, Colors.Yellow, orange, Colors.Green);
            var right = new UVAlignTestPattern(Colors.Red, Colors.Yellow, Colors.Magenta, Colors.Green, Colors.White);
            var back = new UVAlignTestPattern(Colors.Green, Colors.Magenta, Colors.Cyan, Colors.White, Colors.Blue);
            var top = new UVAlignTestPattern(orange, Colors.Cyan, Colors.Magenta, Colors.Red, Colors.Yellow);
            var bottom = new UVAlignTestPattern(Colors.Magenta, orange, Colors.Green, Colors.Blue, Colors.White);
            var map = new CubeMap(left, front, right, back, top, bottom);
            var corner = 0.9f;

            // Left
            map.LocalColorAt(new Point(-1, 0, 0)).Should().Be(Colors.Yellow);
            map.LocalColorAt(new Point(-1, corner, -corner)).Should().Be(Colors.Cyan);
            map.LocalColorAt(new Point(-1, corner, corner)).Should().Be(Colors.Red);
            map.LocalColorAt(new Point(-1, -corner, -corner)).Should().Be(Colors.Blue);
            map.LocalColorAt(new Point(-1, -corner, corner)).Should().Be(orange);

            // Front
            map.LocalColorAt(new Point(0, 0, 1)).Should().Be(Colors.Cyan);
            map.LocalColorAt(new Point(corner, -corner, 1)).Should().Be(Colors.Green);
            map.LocalColorAt(new Point(corner, corner, 1)).Should().Be(Colors.Yellow);
            map.LocalColorAt(new Point(-corner, -corner, 1)).Should().Be(orange);
            map.LocalColorAt(new Point(-corner, corner, 1)).Should().Be(Colors.Red);

            // Right
            map.LocalColorAt(new Point(1, 0, 0)).Should().Be(Colors.Red);
            map.LocalColorAt(new Point(1, corner, -corner)).Should().Be(Colors.Magenta);
            map.LocalColorAt(new Point(1, corner, corner)).Should().Be(Colors.Yellow);
            map.LocalColorAt(new Point(1, -corner, -corner)).Should().Be(Colors.White);
            map.LocalColorAt(new Point(1, -corner, corner)).Should().Be(Colors.Green);

            // Back
            map.LocalColorAt(new Point(0, 0, -1)).Should().Be(Colors.Green);
            map.LocalColorAt(new Point(corner, -corner, -1)).Should().Be(Colors.White);
            map.LocalColorAt(new Point(corner, corner, -1)).Should().Be(Colors.Magenta);
            map.LocalColorAt(new Point(-corner, -corner, -1)).Should().Be(Colors.Blue);
            map.LocalColorAt(new Point(-corner, corner, -1)).Should().Be(Colors.Cyan);

            // Top
            map.LocalColorAt(new Point(0, 1, 0)).Should().Be(orange);
            map.LocalColorAt(new Point(corner, 1, -corner)).Should().Be(Colors.Magenta);
            map.LocalColorAt(new Point(corner, 1, corner)).Should().Be(Colors.Yellow);
            map.LocalColorAt(new Point(-corner, 1,  -corner)).Should().Be(Colors.Cyan);
            map.LocalColorAt(new Point(-corner, 1, corner)).Should().Be(Colors.Red);

            // Bottom
            map.LocalColorAt(new Point(0, -1, 0)).Should().Be(Colors.Magenta);
            map.LocalColorAt(new Point(corner, -1, -corner)).Should().Be(Colors.White);
            map.LocalColorAt(new Point(corner, -1, corner)).Should().Be(Colors.Green);
            map.LocalColorAt(new Point(-corner, -1, -corner)).Should().Be(Colors.Blue);
            map.LocalColorAt(new Point(-corner, -1, corner)).Should().Be(orange);
        }
    }
}