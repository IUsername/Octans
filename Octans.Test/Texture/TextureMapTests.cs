using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class TextureMapTests
    {
        [Fact]
        public void MapPatternToSphere()
        {
            var c = new UVCheckers(16, 8, Colors.Black, Colors.White);
            var tm = new TextureMap(c, UVMapping.Spherical);
            tm.PatternAt(new Point(0.4315f, 0.4670f, 0.7719f)).Should().Be(Colors.White);
            tm.PatternAt(new Point(-0.9654f, 0.2552f, -0.0534f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(0.1039f, 0.7090f, 0.6975f)).Should().Be(Colors.White);
            tm.PatternAt(new Point(-0.4986f, -0.7856f, -0.3663f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(-0.0317f, -0.9395f, 0.3411f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(0.4809f, -0.7721f, 0.4154f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(0.0285f, -0.9612f, -0.2745f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(-0.5734f, -0.2162f, -0.7903f)).Should().Be(Colors.White);
            tm.PatternAt(new Point(0.7688f, -0.1470f, 0.6223f)).Should().Be(Colors.Black);
            tm.PatternAt(new Point(-0.7652f, 0.2175f, 0.6060f)).Should().Be(Colors.Black);
        }
    }
}