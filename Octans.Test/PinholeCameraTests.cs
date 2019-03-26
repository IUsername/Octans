using System;
using FluentAssertions;
using Octans.Shading;
using Xunit;

namespace Octans.Test
{
    public class PinholeCameraTests
    {
        [Fact]
        public void ConstructingCamera()
        {
            var c = new PinholeCamera(in Matrix.Identity, MathF.PI / 2f, 160, 120);
            c.HSize.Should().Be(160);
            c.VSize.Should().Be(120);
            c.FieldOfView.Should().Be(MathF.PI / 2f);
            c.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void HorizontalCanvasPixelSize()
        {
            var c = new PinholeCamera(in Matrix.Identity, MathF.PI / 2f, 200, 125);
            c.PixelSize.Should().Be(0.01f);
        }

        [Fact]
        public void VerticalCanvasPixelSize()
        {
            var c = new PinholeCamera(in Matrix.Identity, MathF.PI / 2f, 125, 200);
            c.PixelSize.Should().Be(0.01f);
        }

        [Fact]
        public void RayThroughCenterOfCanvas()
        {
            var c = new PinholeCamera(in Matrix.Identity, MathF.PI / 2f, 201, 101);
            var r = c.PixelToRay(SubPixel.ForPixelCenter(100, 50));
            r.Origin.Should().Be(Point.Zero);
            r.Direction.Should().Be(new Vector(0, 0, -1));
        }

        [Fact]
        public void RayThroughCornerOfCanvas()
        {
            var c = new PinholeCamera(in Matrix.Identity, MathF.PI / 2f, 201, 101);
            var r = c.PixelToRay(SubPixel.ForPixelCenter(0, 0));
            r.Origin.Should().Be(Point.Zero);
            r.Direction.Should().Be(new Vector(0.66519f, 0.33259f, -0.66851f));
        }

        [Fact]
        public void RayAfterCameraTransform()
        {
            var transform = Transforms.RotateY(MathF.PI / 4f) * Transforms.Translate(0, -2, 5);
            var c = new PinholeCamera(transform, MathF.PI / 2f, 201, 101);
            var r = c.PixelToRay(SubPixel.ForPixelCenter(100, 50));
            r.Origin.Should().Be(new Point(0, 2, -5));
            r.Direction.Should().Be(new Vector(MathF.Sqrt(2f) / 2f, 0.0f, -MathF.Sqrt(2f) / 2f));
        }

        [Fact]
        public void RenderToCanvas()
        {
            var from = new Point(0, 0, -5);
            var to = Point.Zero;
            var up = new Vector(0, 1, 0);
            var transform = Transforms.View(from, to, up);
            var w = World.Default();
            var width = 11;
            var height = 11;
            var c = new PinholeCamera(transform, MathF.PI / 2f, width, height);
            var s = new Scene(c, new PhongWorldShading(1,w));
            var canvas = new Canvas(width,height);
            RenderContext.Render(canvas, s);
            canvas.PixelAt(5, 5).Should().Be(new Color(0.38066f, 0.47583f, 0.2855f));
        }
    }
}