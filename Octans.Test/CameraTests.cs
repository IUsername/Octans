using System;
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
            r.Origin.Should().Be(new Point(0, 2, -5));
            r.Direction.Should().Be(new Vector(MathF.Sqrt(2f) / 2f, 0.0f, -MathF.Sqrt(2f) / 2f));
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

        [Fact]
        public void RenderToCanvasWithAdaptiveAntiAliasing()
        {
            var w = World.Default();
            var c = new Camera(11, 11, MathF.PI / 2f);
            var from = new Point(0, 0, -5);
            var to = Point.Zero;
            var up = new Vector(0, 1, 0);
            c.SetTransform(Transforms.View(from, to, up));
            var image = c.Render(w, 2, 0.03f);
            var p = image.PixelAt(5, 5);
            p.Should().NotBe(new Color(0.38066f, 0.47583f, 0.2855f));
            p.Red.Should().BeApproximately(0.3429f, 0.005f);
            p.Green.Should().BeApproximately(0.4287f, 0.005f);
            p.Blue.Should().BeApproximately(0.2572f, 0.005f);
        }

        [Fact]
        public void SubPixelAlignToLowestDivisions()
        {
            var sp = Camera.SubPixel.Create(1, 1, 1, 1, 1);
            sp.X.Should().Be(2);
            sp.Y.Should().Be(2);
            sp.Divisions.Should().Be(1);
            sp.Dx.Should().Be(0);
            sp.Dy.Should().Be(0);

            sp = Camera.SubPixel.Create(1, 1, 2, 1, 1);
            sp.X.Should().Be(1);
            sp.Y.Should().Be(1);
            sp.Divisions.Should().Be(2);
            sp.Dx.Should().Be(1);
            sp.Dy.Should().Be(1);

            sp = Camera.SubPixel.Create(1, 3, 4, 3, 4);
            sp.X.Should().Be(1);
            sp.Y.Should().Be(4);
            sp.Divisions.Should().Be(4);
            sp.Dx.Should().Be(3);
            sp.Dy.Should().Be(0);
        }

        [Fact]
        public void CanFindSubPixelCorners()
        {
            var sp = Camera.SubPixel.Create(0, 0, 2, 1, 1);
            var (tl, tr, bl, br) = Camera.SubPixel.Corners(in sp);
            tl.Should().Be(Camera.SubPixel.Create(0, 0, 1, 0, 0));
            tr.Should().Be(Camera.SubPixel.Create(1, 0, 1, 0, 0));
            bl.Should().Be(Camera.SubPixel.Create(0, 1, 1, 0, 0));
            br.Should().Be(Camera.SubPixel.Create(1, 1, 1, 0, 0));

            sp = Camera.SubPixel.Create(3, 4, 4, 3, 2);
            (tl, tr, bl, br) = Camera.SubPixel.Corners(in sp);
            tl.Should().Be(Camera.SubPixel.Create(3, 4, 4, 2, 1));
            tr.Should().Be(Camera.SubPixel.Create(4, 4, 4, 0, 1));
            bl.Should().Be(Camera.SubPixel.Create(3, 4, 4, 2, 3));
            br.Should().Be(Camera.SubPixel.Create(4, 4, 4, 0, 3));
        }

        [Fact]
        public void FindsCenterFromTwoSubPixels()
        {
            var tl = Camera.SubPixel.Create(0, 0, 1, 0, 0);
            var br = Camera.SubPixel.Create(1, 1, 1, 0, 0);
            var c = Camera.SubPixel.Center(in tl, in br);
            c.Should().Be(Camera.SubPixel.Create(0, 0, 2, 1, 1));

            tl = Camera.SubPixel.Create(2, 3, 2, 1, 1);
            br = Camera.SubPixel.Create(3, 4, 1, 0, 0);
            c = Camera.SubPixel.Center(in tl, in br);
            c.Should().Be(Camera.SubPixel.Create(2, 3, 4, 3, 3));
        }
    }
}