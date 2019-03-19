using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class PatternBaseTests
    {
        [Fact]
        public void DefaultTransform()
        {
            var pattern = new TestPattern();
            pattern.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void CanSetTransform()
        {
            var pattern = new TestPattern();
            pattern.SetTransform(Transforms.Translate(1, 2, 3));
            pattern.Transform.Should().Be(Transforms.Translate(1, 2, 3));
        }

        [Fact]
        public void PatternWithObjectTransform()
        {
            var obj = new Sphere();
            obj.SetTransform(Transforms.Scale(2, 2, 2));
            var pattern = new TestPattern();
            pattern.ShapeColor(obj, new Point(2f, 3f, 4f)).Should().Be(new Color(1f, 1.5f, 2f));
        }

        [Fact]
        public void PatternWithPatternTransform()
        {
            var obj = new Sphere();
            var pattern = new TestPattern();
            pattern.SetTransform(Transforms.Scale(2, 2, 2));
            pattern.ShapeColor(obj, new Point(2f, 3f, 4f)).Should().Be(new Color(1f, 1.5f, 2f));
        }

        [Fact]
        public void PatternWithObjectAndPatternTransform()
        {
            var obj = new Sphere();
            obj.SetTransform(Transforms.Scale(2, 2, 2));
            var pattern = new TestPattern();
            pattern.SetTransform(Transforms.Translate(0.5f, 1, 1.5f));
            pattern.ShapeColor(obj, new Point(2.5f, 3f, 3.5f)).Should().Be(new Color(0.75f, 0.5f, 0.25f));
        }
    }

    internal class TestPattern : PatternBase
    {
        public override Color LocalColorAt(Point localPoint) => new Color(localPoint.X, localPoint.Y, localPoint.Z);
    }
}