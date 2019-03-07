using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class CameraTests
    {
        [Fact]
        public void ConstructingCamera()
        {
            var c = new Camera(160, 120, MathF.PI / 2f);
            c.HSize.Should().Be(160);
            c.VSize.Should().Be(120);
            c.FieldOfView.Should().Be(MathF.PI / 2f);
            c.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void HorizontalCanvasPixelSize()
        {
            var c = new Camera(200, 125, MathF.PI / 2f);
            c.PixelSize.Should().Be(0.01f);
        }

        [Fact]
        public void VerticalCanvasPixelSize()
        {
            var c = new Camera(125, 200, MathF.PI / 2f);
            c.PixelSize.Should().Be(0.01f);
        }

        [Fact]
        public void RayThroughCenterOfCanvas()
        {
            var c = new Camera(201, 101, MathF.PI / 2f);
            var r = c.RayForPixel(100, 50);
            r.Origin.Should().Be(Point.Zero);
            r.Direction.Should().Be(new Vector(0, 0, -1));
        }

        [Fact]
        public void RayThroughCornerOfCanvas()
        {
            var c = new Camera(201, 101, MathF.PI / 2f);
            var r = c.RayForPixel(0, 0);
            r.Origin.Should().Be(Point.Zero);
            r.Direction.Should().Be(new Vector(0.66519f, 0.33259f, -0.66851f));
        }

        [Fact]
        public void RayAfterCameraTransform()
        {
            var c = new Camera(201, 101, MathF.PI / 2f);
            c.SetTransform(Transforms.RotateY(MathF.PI / 4f) * Transforms.Translate(0, -2, 5));
            var r = c.RayForPixel(100, 50);
            r.Origin.Should().Be(new Point(0,2,-5));
            r.Direction.Should().Be(new Vector(MathF.Sqrt(2f)/ 2f, 0.0f, -MathF.Sqrt(2f) / 2f));
        }

        [Fact]
        public void RenderToCanvas()
        {
            var w = World.Default();
            var c = new Camera(11, 11, MathF.PI / 2f);
            var from = new Point(0, 0, -5);
            var to = Point.Zero;
            var up = new Vector(0, 1, 0);
            c.SetTransform(Transforms.View(from, to, up));
            var image = c.Render(w);
            image.PixelAt(5, 5).Should().Be(new Color(0.38066f, 0.47583f, 0.2855f));

        }
    }
}
