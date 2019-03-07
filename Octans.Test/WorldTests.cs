using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class WorldTests
    {
        [Fact]
        public void DefaultWorld()
        {
            var world = World.Default();
            world.Objects.Should().HaveCount(2);
            world.Lights.Should().HaveCount(1);
        }

        [Fact]
        public void IntersectWorldWithRay()
        {
            var w = World.Default();
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var xs = w.Intersect(r);
            xs.Should().HaveCount(4);
            xs[0].T.Should().Be(4f);
            xs[1].T.Should().Be(4.5f);
            xs[2].T.Should().Be(5.5f);
            xs[3].T.Should().Be(6f);
        }

        [Fact(Skip = "creates file in My Pictures folder")]
        public void TestRender()
        {
            var floor = new Sphere();
            floor.SetTransform(Transforms.Scale(10, 0.01f, 10));
            floor.Material.Color = new Color(1f, 0.9f, 0.9f);
            floor.Material.Specular = 0f;

            var leftWall = new Sphere();
            leftWall.SetTransform(
                Transforms.Translate(0, 0, 5)
                * Transforms.RotateY(-MathF.PI / 4f)
                * Transforms.RotateX(MathF.PI / 2f)
                * Transforms.Scale(10, 0.01f, 10));
            leftWall.SetMaterial(floor.Material);

            var rightWall = new Sphere();
            rightWall.SetTransform(
                Transforms.Translate(0, 0, 5)
                * Transforms.RotateY(MathF.PI / 4f)
                * Transforms.RotateX(MathF.PI / 2f)
                * Transforms.Scale(10, 0.01f, 10));
            rightWall.SetMaterial(floor.Material);

            var middle = new Sphere();
            middle.SetTransform(Transforms.Translate(-0.5f, 1f, 0.5f));
            middle.Material.Color = new Color(0.1f, 1f, 0.5f);
            middle.Material.Diffuse = 0.7f;
            middle.Material.Specular = 0.3f;

            var right = new Sphere();
            right.SetTransform(Transforms.Translate(1.5f, 0.5f, -0.5f) * Transforms.Scale(0.5f, 0.5f, 0.5f));
            right.Material.Color = new Color(0.5f, 1f, 0.1f);
            right.Material.Diffuse = 0.7f;
            right.Material.Specular = 0.3f;

            var left = new Sphere();
            left.SetTransform(Transforms.Translate(-1.5f, 0.33f, -0.75f) * Transforms.Scale(0.33f, 0.33f, 0.33f));
            left.Material.Color = new Color(1f, 0.8f, 0.1f);
            left.Material.Diffuse = 0.7f;
            left.Material.Specular = 0.3f;

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(floor, leftWall, rightWall, middle, right, left);

            var c = new Camera(100, 50, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
        }
    }
}