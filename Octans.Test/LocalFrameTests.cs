using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class LocalFrameTests
    {
        [Fact]
        public void CanRoundtripLocalFrame()
        {
            var brdfN = new Vector(0, 1, 0);
            var frame = new LocalFrame(in brdfN);
            var localV = new Vector(0, 0, 1).Normalize();
            var worldV = frame.ToWorld(in localV);
            worldV.Should().Be(new Vector(0, 1, 0));
            localV = frame.ToLocal(in worldV);
            localV.Should().Be(new Vector(0, 0, 1));
            worldV = new Vector(1, 2, 3).Normalize();
            localV = frame.ToLocal(in worldV);
            frame.ToWorld(in localV).Should().Be(worldV);
        }

        [Fact]
        public void NoImpactIfWorldAndLocalNormalsMatch()
        {
            var brdfN = new Vector(0, 0, 1);
            var frame = new LocalFrame(in brdfN);
            var localV = new Vector(0, 0, 1).Normalize();
            var worldV = frame.ToWorld(in localV);
            worldV.Should().Be(new Vector(0, 0, 1));
            localV = new Vector(2, 3, 4).Normalize();
            worldV = frame.ToWorld(in localV);
            worldV.Should().Be(localV);
        }

        [Fact]
        public void CanHandleNegativeZValues()
        {
            var brdfN = new Vector(0, 3, -1).Normalize();
            var frame = new LocalFrame(in brdfN);
            var localV = new Vector(1, 0, 2).Normalize();
            var worldV = frame.ToWorld(in localV);
            frame.ToLocal(in worldV).Should().Be(localV);
        }
    }
}