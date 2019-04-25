using FluentAssertions;
using Xunit;

namespace Octans.Test
{
   public class SpectrumTests
    {
        [Fact]
        public void Add()
        {
            var s1 = new Spectrum(0.5f);
            var s2 = new Spectrum(0.1f);
            var s3 = s1 + s2;
            s3[0].Should().Be(0.6f);
            s3[1].Should().Be(0.6f);
            s3[Spectrum.Samples - 1].Should().Be(0.6f);
        }

        [Fact]
        public void Subtract()
        {
            var s1 = new Spectrum(0.5f);
            var s2 = new Spectrum(0.1f);
            var s3 = s1 - s2;
            s3[0].Should().Be(0.4f);
            s3[1].Should().Be(0.4f);
            s3[Spectrum.Samples - 1].Should().Be(0.4f);
        }

        [Fact]
        public void Multiply()
        {
            var s1 = new Spectrum(1f);
            var s2 = new Spectrum(0.1f);
            var s3 = s1 * s2;
            s3[0].Should().Be(0.1f);
            s3[1].Should().Be(0.1f);
            s3[Spectrum.Samples - 1].Should().Be(0.1f);
        }

        [Fact]
        public void MultiplyScalar()
        {
            var s1 = new Spectrum(1f);
            var s3 = s1 * 0.1f;
            s3[0].Should().Be(0.1f);
            s3[1].Should().Be(0.1f);
            s3[Spectrum.Samples - 1].Should().Be(0.1f);
        }

        [Fact]
        public void Divide()
        {
            var s1 = new Spectrum(1f);
            var s2 = new Spectrum(2f);
            var s3 = s1 / s2;
            s3[0].Should().Be(0.5f);
            s3[1].Should().Be(0.5f);
            s3[Spectrum.Samples - 1].Should().Be(0.5f);
        }

        [Fact]
        public void DivideScalar()
        {
            var s1 = new Spectrum(1f);
            var s3 = s1 / 2f;
            s3[0].Should().Be(0.5f);
            s3[1].Should().Be(0.5f);
            s3[Spectrum.Samples - 1].Should().Be(0.5f);
        }

        [Fact]
        public void Sqrt()
        {
            var s1 = new Spectrum(9f);
            var s3 = s1.Sqrt();
            s3[0].Should().Be(3f);
            s3[1].Should().Be(3f);
            s3[Spectrum.Samples - 1].Should().Be(3f);
        }

        [Fact]
        public void Pow()
        {
            var s1 = new Spectrum(2f);
            var s3 = s1.Pow(5);
            s3[0].Should().Be(32f);
            s3[1].Should().Be(32f);
            s3[Spectrum.Samples - 1].Should().Be(32f);
        }
    }

  
}
