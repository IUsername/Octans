using FluentAssertions;
using Octans.Sampling;
using Xunit;

namespace Octans.Test.Sampling
{
    public class HaltonSamplerTests
    {
        [Fact]
        public void Initialize()
        {
            var area = new PixelArea(new PixelCoordinate(0,0), new PixelCoordinate(100,100));
            var hs = new HaltonSampler(10, area);
            hs.SamplesPerPixel.Should().Be(10);
        }

        [Fact]
        public void GetCameraSampleForPixels()
        {
            var start = new PixelCoordinate(0, 0);
            var end = new PixelCoordinate(100, 100);
            var area = new PixelArea(start, end);
            var hs = new HaltonSampler(10, area);
            hs.StartPixel(in start);
            var cs = hs.GetCameraSample(in start);
            cs.FilmPoint.X.Should().BeInRange(0f, 1f);
            cs.FilmPoint.Y.Should().BeInRange(0f, 1f);
            cs.LensPoint.X.Should().BeInRange(0f, 1f);
            cs.LensPoint.Y.Should().BeInRange(0f, 1f);

            var next = new PixelCoordinate(1,0);
            hs.StartPixel(in next);
            var csn = hs.GetCameraSample(in next);
            csn.FilmPoint.X.Should().BeInRange(1f, 2f);
            csn.FilmPoint.Y.Should().BeInRange(0f, 1f);
            csn.LensPoint.X.Should().BeInRange(0f, 1f);
            csn.LensPoint.Y.Should().BeInRange(0f, 1f);

            cs.FilmPoint.X.Should().NotBe(csn.FilmPoint.X);
            cs.FilmPoint.Y.Should().NotBe(csn.FilmPoint.Y);
            cs.LensPoint.X.Should().NotBe(csn.LensPoint.X);
            cs.LensPoint.Y.Should().NotBe(csn.LensPoint.Y);
        }
    }
}