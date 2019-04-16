using System;
using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class CheckerCompositeTextureTests
    {
        [Fact]
        public void RepeatsInX()
        {
            var texture = new CheckerCompositeTexture(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInY()
        {
            var texture = new CheckerCompositeTexture(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0.99f, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 1.01f, 0)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0, 2.01f, 0)).Should().Be(Colors.White);
        }

        [Fact]
        public void RepeatsInZ()
        {
            var texture = new CheckerCompositeTexture(new SolidColor(Colors.White), new SolidColor(Colors.Black));
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 0.99f)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0, 0, 1.01f)).Should().Be(Colors.Black);
            texture.LocalColorAt(new Point(0, 0, 2.01f)).Should().Be(Colors.White);
        }

        [Fact]
        public void RespectsNestedTransform()
        {
            var blue = new Color(0, 0, 1);
            var stripe = new StripeTexture(Colors.Black, blue);
            var texture = new CheckerCompositeTexture(new SolidColor(Colors.White), stripe);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(blue);
            texture.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(2.01f, 1.01f, 0)).Should().Be(Colors.Black);

            stripe.SetTransform(Transform.RotateY(System.MathF.PI / 2));
            texture = new CheckerCompositeTexture(new SolidColor(Colors.White), stripe);
            texture.LocalColorAt(new Point(0, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(0.99f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(1.01f, 0, 0)).Should().Be(blue);
            texture.LocalColorAt(new Point(2.01f, 0, 0)).Should().Be(Colors.White);
            texture.LocalColorAt(new Point(2.01f, 1.01f, 0)).Should().Be(blue);
        }
    }
}