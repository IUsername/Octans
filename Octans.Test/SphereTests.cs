using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SphereTests
    {
        [Fact]
        public void NonEdgeRayIntersectsSphereAtTwoPoints()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(4.0f);
            xs[1].T.Should().Be(6.0f);
            xs[0].Obj.Should().Be(s);
            xs[1].Obj.Should().Be(s);
        }

        [Fact]
        public void NonIntersectionReturnsZeroCount()
        {
            var r = new Ray(Point.Create(0, 2, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(0);
        }

        [Fact]
        public void RayOriginatingInsideSphereHasOneNegativeIntersect()
        {
            var r = new Ray(Point.Create(0, 0, 0), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-1.0f);
            xs[1].T.Should().Be(1.0f);
        }

        [Fact]
        public void SphereBehindRayHasTwoNegativeIntersects()
        {
            var r = new Ray(Point.Create(0, 0, 5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(-6.0f);
            xs[1].T.Should().Be(-4.0f);
        }

        [Fact]
        public void DefaultTransformIsIdentity()
        {
            var s = new Sphere();
            s.Transform.Should().Be(Matrix.Identity);
        }

        [Fact]
        public void CanChangeTransform()
        {
            var s = new Sphere();
            var t = Transforms.Translate(2, 3, 4);
            s.SetTransform(t);
            s.Transform.Should().Be(t);
        }

        [Fact]
        public void IntersectingScaledSphere()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var t = Transforms.Scale(2, 2, 2);
            s.SetTransform(t);
            var xs = s.Intersect(r);
            xs.Should().HaveCount(2);
            xs[0].T.Should().Be(3f);
            xs[1].T.Should().Be(7f);
        }

        [Fact]
        public void IntersectingTranslatedSphere()
        {
            var r = new Ray(Point.Create(0, 0, -5), Vector.Create(0, 0, 1));
            var s = new Sphere();
            var t = Transforms.Translate(5, 0, 0);
            s.SetTransform(t);
            var xs = s.Intersect(r);
            xs.Should().HaveCount(0);
        }

        [Fact(Skip = "creates file in My Pictures folder")]
        public void RaycastTest()
        {
            const int canvasPixels = 100;
            var canvas = new Canvas(canvasPixels, canvasPixels);
            var color = Color.RGB(1f, 0f, 0f);
            var s = new Sphere();
            var rayOrigin = Point.Create(0f, 0f, -5f);
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
                    var position = Point.Create(worldX, worldY, wallZ);
                    var r = new Ray(rayOrigin, (position - rayOrigin).Normalize());
                    var xs = s.Intersect(r);
                    if (xs.Hit().HasValue)
                    {
                        canvas.WritePixel(color, x, y);
                    }
                }
            }

            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "raycast");
        }
    }
}