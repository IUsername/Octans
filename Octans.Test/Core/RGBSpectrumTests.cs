using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class RGBSpectrumTests
    {
        [Fact]
        public void CreateFromSingleValue()
        {
            var s = new RGBSpectrum(1f);
            RGBSpectrum.ToColor(s).Should().Be(Colors.White);
        }

        [Fact]
        public void ToFromColor()
        {
            var c = Colors.Red;
            var s = RGBSpectrum.FromColor(in c);
            s[0].Should().Be(1f);
            s[1].Should().Be(0f);
            s[2].Should().Be(0f);
            var c1 = RGBSpectrum.ToColor(in s);
            c1.Should().Be(c);
        }

        [Fact]
        public void Add()
        {
            var s1 = RGBSpectrum.FromColor(Colors.Green);
            var s2 = RGBSpectrum.FromColor(Colors.Red);
            var s3 = s1 + s2;
            RGBSpectrum.ToColor(s3).Should().Be(Colors.Yellow);
        }

        [Fact]
        public void Subtract()
        {
            var s1 = RGBSpectrum.FromColor(Colors.Yellow);
            var s2 = RGBSpectrum.FromColor(Colors.Red);
            var s3 = s1 - s2;
            RGBSpectrum.ToColor(s3).Should().Be(Colors.Green);
        }

        [Fact]
        public void Multiply()
        {
            var y = RGBSpectrum.FromColor(Colors.Yellow);
            var r = RGBSpectrum.FromColor(Colors.Red);
            var m = y * r;
            RGBSpectrum.ToColor(m).Should().Be(Colors.Red);
        }

        [Fact]
        public void MultiplyScalar()
        {
            var y = RGBSpectrum.FromColor(Colors.Yellow);
            var m = y * 2f;
            m[0].Should().Be(2f);
            m[1].Should().Be(2f);
            m[2].Should().Be(0f);
        }

        [Fact]
        public void DivideScalar()
        {
            var y = RGBSpectrum.FromColor(Colors.Yellow);
            var m = y / 2f;
            m[0].Should().Be(0.5f);
            m[1].Should().Be(0.5f);
            m[2].Should().Be(0f);
        }

        [Fact]
        public void EqualityCheck()
        {
            var a = RGBSpectrum.FromColor(Colors.Yellow);
            var b = RGBSpectrum.FromColor(Colors.Red);
            var c = RGBSpectrum.FromColor(Colors.Yellow);
            (a == b).Should().BeFalse();
            (a != b).Should().BeTrue();
            (a == c).Should().BeTrue();
            (a != c).Should().BeFalse();
        }

        [Fact]
        public void IsBlack()
        {
            var a = RGBSpectrum.FromColor(Colors.Yellow);
            a.IsBlack().Should().BeFalse();
            var b = new RGBSpectrum();
            b.IsBlack().Should().BeTrue();
        }
    }
}
