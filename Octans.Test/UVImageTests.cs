using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class UVImageTests
    {
        [Fact]
        public void ReturnsColorByUV()
        {
            var ppm = @"P3
10 10
10
0 0 0  1 1 1  2 2 2  3 3 3  4 4 4  5 5 5  6 6 6  7 7 7  8 8 8  9 9 9
1 1 1  2 2 2  3 3 3  4 4 4  5 5 5  6 6 6  7 7 7  8 8 8  9 9 9  0 0 0
2 2 2  3 3 3  4 4 4  5 5 5  6 6 6  7 7 7  8 8 8  9 9 9  0 0 0  1 1 1
3 3 3  4 4 4  5 5 5  6 6 6  7 7 7  8 8 8  9 9 9  0 0 0  1 1 1  2 2 2
4 4 4  5 5 5  6 6 6  7 7 7  8 8 8  9 9 9  0 0 0  1 1 1  2 2 2  3 3 3
5 5 5  6 6 6  7 7 7  8 8 8  9 9 9  0 0 0  1 1 1  2 2 2  3 3 3  4 4 4
6 6 6  7 7 7  8 8 8  9 9 9  0 0 0  1 1 1  2 2 2  3 3 3  4 4 4  5 5 5
7 7 7  8 8 8  9 9 9  0 0 0  1 1 1  2 2 2  3 3 3  4 4 4  5 5 5  6 6 6
8 8 8  9 9 9  0 0 0  1 1 1  2 2 2  3 3 3  4 4 4  5 5 5  6 6 6  7 7 7
9 9 9  0 0 0  1 1 1  2 2 2  3 3 3  4 4 4  5 5 5  6 6 6  7 7 7  8 8 8
";
            var canvas = PPM.Parse(ppm);
            var pattern = new UVImage(canvas);
            pattern.PatternAt(0f, 0f).Should().Be(new Color(0.9f, 0.9f, 0.9f));
            pattern.PatternAt(0.3f, 0f).Should().Be(new Color(0.2f, 0.2f, 0.2f));
            pattern.PatternAt(0.6f, 0.3f).Should().Be(new Color(0.1f, 0.1f, 0.1f));
            pattern.PatternAt(1f, 1f).Should().Be(new Color(0.9f, 0.9f, 0.9f));
        }
    }
}