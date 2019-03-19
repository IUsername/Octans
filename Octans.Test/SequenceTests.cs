using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SequenceTests
    {
        [Fact]
        public void LoopingEnumerator()
        {
            var gen = new Sequence(0.1f, 0.5f, 1.0f);
            gen.Next().Should().Be(0.1f);
            gen.Next().Should().Be(0.5f);
            gen.Next().Should().Be(1.0f);
            gen.Next().Should().Be(0.1f);
        }
    }
}