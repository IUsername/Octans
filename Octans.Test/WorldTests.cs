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

        [Fact(Skip ="Slow to render")]
        public void TestRender()
        {

            var s1 = new StripePattern(Colors.White, Colors.Black);
            var s2 = new StripePattern(Colors.White, Colors.Black);
            s2.SetTransform(Transforms.RotateY(MathF.PI/2));
            var pattern = new BlendedCompositePattern(s1, s2);

            var stripe = new StripePattern(new Color(0.9f, 0, 0), new Color(0.0f, 0.0f, 0.9f));
            stripe.SetTransform(Transforms.Scale(0.25f, 0.25f, 0.25f).RotateY(MathF.PI / 4));
            var perlin = new PerlinRippleCompositePattern(stripe, 0.8f);
            perlin.SetTransform(Transforms.Scale(0.1f, 0.1f, 0.1f));

            var floor = new Plane {Material =
            {
                Pattern = pattern,// new CheckerPattern(new Color(0.75f, 0.75f, 0.75f), Colors.White),
                Specular = 0f
            }};

            var middle = new Sphere {Material = { Pattern = perlin, Diffuse = 0.7f, Specular = 0.3f, Reflective = 0.4f}};
            middle.SetTransform(Transforms.Translate(-0.5f, 1f, 0.5f));
            //middle.SetTransform(Transforms.Translate(-0.5f, 0.5f, 0.5f));

            var right = new Sphere {Material = { Pattern = new SolidColor(new Color(0.5f, 1f, 0.1f)), Diffuse = 0.7f, Specular = 0.3f, Reflective = 0.2f}};
            right.SetTransform(Transforms.Translate(1.5f, 0.5f, -0.5f) * Transforms.Scale(0.5f, 0.5f, 0.5f));

            var left = new Sphere {Material = { Pattern = new SolidColor(new Color(0.9f, 0.9f, 1f)), Diffuse = 0.1f, Specular = 0.9f, Transparency = 0.95f, RefractiveIndex=1.52f, Reflective = 1.5f, Ambient = 0.11f, Shininess = 300}};
            left.SetTransform(Transforms.Translate(-1.6f, 0.33f, -0.75f) * Transforms.Scale(0.33f, 0.33f, 0.33f));

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(floor, middle, right, left);

            var c = new Camera(300, 200, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "scene");
        }
    }
}