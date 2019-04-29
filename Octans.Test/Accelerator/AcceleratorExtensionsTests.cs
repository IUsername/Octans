using FluentAssertions;
using Octans.Accelerator;
using Xunit;

namespace Octans.Test.Accelerator
{
    public class AcceleratorExtensionsTests
    {
        [Fact]
        public void Partition()
        {
            var data = new[] {3, 4, 1, 7, 2, 9, 0, 16, 3};
            var pivot = data.Partition(0, data.Length, i => i < 5);
            pivot.Should().Be(5);
            var lessThanFive = data[..pivot];
            lessThanFive.Should().OnlyContain(i => i < 5);
            var fiveOrGreater = data[pivot + 1..];
            fiveOrGreater.Should().OnlyContain(i => i >= 5);
        }

        [Fact]
        public void Partition2()
        {
            var data = new[] { 3,2,1,4,5,8 };
            var pivot = data.Partition(0, data.Length, i => i <= 1);
            pivot.Should().Be(1);
            var before = data[..pivot];
            before.Should().OnlyContain(i => i <= 1);
            var after = data[pivot + 1..];
            after.Should().OnlyContain(i => i > 1);
        }

        [Fact]
        public void NthElement()
        {
            var data = new[] {3, 4, 1, 7, 2, 9, 0, 16, 3};
            data.NthElement(0, data.Length, 5, (a, b) => a.CompareTo(b));
            data[5].Should().Be(4);
            var lessOrEqToNth = data[..5];
            lessOrEqToNth.Should().OnlyContain(i => i <= 4);
            var greaterThanNth = data[5 + 1..];
            greaterThanNth.Should().OnlyContain(i => i > 4);
        }

        [Fact]
        public void HeapSelect()
        {
            var data = new[] {3, 4, 1, 7, 2, 9, 0, 16, 3};
            data.HeapSelect(4, 0, data.Length - 1, (a, b) => a.CompareTo(b)).Should().Be(4);
            data.HeapSelect(5, 0, data.Length - 1, (a, b) => a.CompareTo(b)).Should().Be(3);
        }

        [Fact]
        public void MortonEncode()
        {
            var v = new Vector(5, 9, 1);
            var result = v.EncodeMorton3();
            result.Should().Be(1095u);
        }
    }
}