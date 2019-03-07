using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class MaterialTests
    {
        [Fact]
        public void ContainsColorAmbientDiffuseSpecularAndShininess()
        {
            var m = new Material();
            m.Color.Should().Be(new Color(1f, 1f, 1f));
            m.Ambient.Should().Be(0.1f);
            m.Diffuse.Should().Be(0.9f);
            m.Specular.Should().Be(0.9f);
            m.Shininess.Should().Be(200f);
        }

        [Fact]
        public void LightingWithPatternApplied()
        {
            var s = new Sphere();
            var m = new Material
            {
                Pattern = new StripePattern(Colors.White, Colors.Black),
                Ambient = 1f,
                Diffuse = 0f,
                Specular = 0f
            };
            var eye = new Vector(0, 0, -1);
            var normal = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), Colors.White);
            var c1 = Shading.Lighting(m, s, light, new Point(0.9f, 0, 0), eye, normal, false);
            var c2 = Shading.Lighting(m, s, light, new Point(1.1f, 0, 0), eye, normal, false);
            c1.Should().Be(Colors.White);
            c2.Should().Be(Colors.Black);
        }
    }
}