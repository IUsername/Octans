using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class CheckerCompositePatternTests
    {
        [Fact]
        public void RepeatsInX()
        {
            var pattern = new CheckerCompositePattern(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInY()
        {
            var pattern = new CheckerCompositePattern(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0.99f, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 1.01f, 0)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0, 2.01f, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInZ()
        {
            var pattern = new CheckerCompositePattern(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 0.99f)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0, 0, 1.01f)).Should().Be(Colors.Black);
            pattern.LocalColorAt(new Point(0, 0, 2.01f)).Should().Be(Colors.White);
        }

        [Fact]
        public void RespectsNestedTransform()
        {
            var blue = new Color(0, 0, 1);
            var stripe = new StripePattern(Colors.Black, blue);
            var pattern = new CheckerCompositePattern(new SolidColor(Colors.White), stripe);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(blue);
            pattern.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(2.01f, 1.01f, 0)).Should().Be(Colors.Black);

            stripe.SetTransform(Transforms.RotateY(MathF.PI/2));
            pattern = new CheckerCompositePattern(new SolidColor(Colors.White), stripe);
            pattern.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(blue);
            pattern.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
            pattern.LocalColorAt(new Point(2.01f, 1.01f, 0)).Should().Be(blue);
        }
    }
}