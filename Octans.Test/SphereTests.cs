using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SphereTests
    {
        [Fact]
        public void IsShape()
        {
            var s = new Sphere();
            s.Should().BeAssignableTo<IShape>();
        }

        [Fact]
        public void NonEdgeRayIntersectsSphereAtTwoPoints()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var s = new Sphere();
            var xs = s.LocalIntersects(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(4.0f);
            xs[1].T.Should().Be(6.0f);
            xs[0].Shape.Should().Be(s);
            xs[1].Shape.Should().Be(s);
        }

        [Fact]
        public void NonIntersectionReturnsZeroCount()
        {
            var r = new Ray(new Point(0, 2, -5), new Vector(0, 0, 1));
            var s = new Sphere();
            var xs = s.LocalIntersects(r);
            xs.Should().HaveCount(0);
        }

        [Fact]
        public void RayOriginatingInsideSphereHasOneNegativeIntersect()
        {
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var s = new Sphere();
            var xs = s.LocalIntersects(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-1.0f);
            xs[1].T.Should().Be(1.0f);
        }

        [Fact]
        public void SphereBehindRayHasTwoNegativeIntersects()
        {
            var r = new Ray(new Point(0, 0, 5), new Vector(0, 0, 1));
            var s = new Sphere();
            var xs = s.LocalIntersects(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-6.0f);
            xs[1].T.Should().Be(-4.0f);
        }

        [Fact]
        public void NormalOnXAxis()
        {
            var s = new Sphere();
            var n = s.LocalNormalAt(new Point(1, 0, 0));
            n.Should().Be(new Vector(1, 0, 0));
        }

        [Fact]
        public void NormalOnYAxis()
        {
            var s = new Sphere();
            var n = s.LocalNormalAt(new Point(0, 1, 0));
            n.Should().Be(new Vector(0, 1, 0));
        }

        [Fact]
        public void NormalOnZAxis()
        {
            var s = new Sphere();
            var n = s.LocalNormalAt(new Point(0, 0, 1));
            n.Should().Be(new Vector(0, 0, 1));
        }

        [Fact(Skip = "creates file in My Pictures folder")]
        public void RaycastTest()
        {
            const int canvasPixels = 100;
            var canvas = new Canvas(canvasPixels, canvasPixels);
            var s = new Sphere {Material = {Color = new Color(0.4f, 0.2f, 1)}};
            var light = new PointLight(new Point(-10, 10, -10), new Color(1f, 1f, 1f));
            //var t = Transforms.Shear(0.1f, 0, 0, 0, 0, 0).Scale(0.9f, 1f, 1f);
            //s.SetTransform(t);
            var rayOrigin = new Point(0f, 0f, -5f);
            const float wallZ = 10f;
            const float wallSize = 7.0f;
            const float pixelSize = wallSize / canvasPixels;
            const float half = wallSize / 2;

            for (var y = 0; y < canvasPixels; y++)
            {
                var worldY = half - pixelSize * y;
                for (var x = 0; x < canvasPixels; x++)
                {
                    var worldX = -half + pixelSize * x;
                    var position = new Point(worldX, worldY, wallZ);
                    var r = new Ray(rayOrigin, (position - rayOrigin).Normalize());
                    var xs = s.Intersects(r);
                    var hit = xs.Hit();
                    if (!hit.HasValue)
                    {
                        continue;
                    }

                    var point = r.Position(hit.Value.T);
                    var shape = hit.Value.Shape;
                    var normal = shape.NormalAt(point);
                    var eye = -r.Direction;
                    var color = Shading.Lighting(shape.Material, shape, light, point, eye, normal, false);
                    canvas.WritePixel(color, x, y);
                }
            }

            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "raycast");
        }
    }
}