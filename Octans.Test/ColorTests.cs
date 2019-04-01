using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ColorTests
    {
        [Fact]
        public void ColorIsRgbTuple()
        {
            var c = new Color(-0.5f, 0.4f, 1.7f);
            c.Red.Should().Be(-0.5f);
            c.Green.Should().Be(0.4f);
            c.Blue.Should().Be(1.7f);
        }

        [Fact]
        public void AddingColors()
        {
            var c1 = new Color(0.9f, 0.6f, 0.75f);
            var c2 = new Color(0.7f, 0.1f, 0.25f);
            (c1 + c2 == new Color(1.6f, 0.7f, 1.0f)).Should().BeTrue();
        }

        [Fact]
        public void SubtractingColors()
        {
            var c1 = new Color(0.9f, 0.6f, 0.75f);
            var c2 = new Color(0.7f, 0.1f, 0.25f);
            (c1 - c2 == new Color(0.2f, 0.5f, 0.5f)).Should().BeTrue();
        }

        [Fact]
        public void MultiplyingByScalar()
        {
            var c = new Color(0.2f, 0.3f, 0.4f);
            (c * 2 == new Color(0.4f, 0.6f, 0.8f)).Should().BeTrue();
        }

        [Fact]
        public void MultiplyingColors()
        {
            var c1 = new Color(1.0f, 0.2f, 0.4f);
            var c2 = new Color(0.9f, 1.0f, 0.1f);
            (c1 * c2 == new Color(0.9f, 0.2f, 0.04f)).Should().BeTrue();
        }

        [Fact]
        public void ColorDeltaRange()
        {
            var white = Colors.White;
            var black = Colors.Black;
            var d = Color.PerceptiveColorDelta(in black, in black);
            d.Should().Be(0f);
            d = Color.PerceptiveColorDelta(in white, in black);
            d.Should().Be(1f);
        }

        [Fact]
        public void GreenHasMoreWeightThanBlue()
        {
           var b1 = new Color(0f,0f,1f);
           var b2 = new Color(0f,0f,0.9f);
            var dB = Color.PerceptiveColorDelta(in b1, in b2);
          
            var g1 = new Color(0f, 1f, 0f);
            var g2 = new Color(0f, 0.9f, 0f);
            var dG = Color.PerceptiveColorDelta(in g1, in g2);

            dG.Should().BeGreaterThan(dB);
        }

        [Fact]
        public void GreenHasMoreWeightThanRed()
        {
            var r1 = new Color(1f, 0f, 0f);
            var r2 = new Color(0.9f, 0f, 0f);
            var dR = Color.PerceptiveColorDelta(in r1, in r2);

            var g1 = new Color(0f, 1f, 0f);
            var g2 = new Color(0f, 0.9f, 0f);
            var dG = Color.PerceptiveColorDelta(in g1, in g2);

            dG.Should().BeGreaterThan(dR);
        }

        [Fact]
        public void BlueHasMoreWeightThanRed()
        {
            var r1 = new Color(1f, 0f, 0f);
            var r2 = new Color(0.9f, 0f, 0f);
            var dR = Color.PerceptiveColorDelta(in r1, in r2);

            var b1 = new Color(0f, 0f, 1f);
            var b2 = new Color(0f, 0f, 0.9f);
            var dB = Color.PerceptiveColorDelta(in b1, in b2);

            dB.Should().BeGreaterThan(dR);
        }

        [Fact]
        public void ColorDeltaWithinLimit()
        {
            Color.IsWithinPerceptiveDelta(Colors.Black, Colors.White, 1f).Should().BeFalse();
            Color.IsWithinPerceptiveDelta(Colors.Black, Colors.White, 1.1f).Should().BeTrue();
        }

        [Fact]
        public void DeltaGreaterForDarkerColors()
        {
            Color.IsWithinPerceptiveDelta(new Color(0.9f, 0.9f, 0.9f), Colors.White, 0.2f).Should().BeTrue();
            Color.IsWithinPerceptiveDelta(new Color(0.1f, 0.1f, 1f), Colors.Blue, 0.2f).Should().BeFalse();
            Color.IsWithinPerceptiveDelta(new Color(0.1f, 0.1f, 0.1f), Colors.Black, 0.2f).Should().BeFalse();
        }

        [Fact]
        public void LerpBetweenColors()
        {
            Color.Lerp(Colors.White, Colors.Black, 0.5f).Should().Be(new Color(0.5f, 0.5f, 0.5f));
            Color.Lerp(Colors.White, Colors.Black, 0.0f).Should().Be(Colors.White);
            Color.Lerp(Colors.White, Colors.Black, 1.0f).Should().Be(Colors.Black);
        }
    }
}