using FluentAssertions;
using Octans.Geometry;
using Octans.Light;
using Octans.Shading;
using Octans.Texture;
using Xunit;

namespace Octans.Test
{
    public class MaterialInfoTests
    {
        [Fact]
        public void ContainsPatternAmbientDiffuseSpecularAndShininess()
        {
            var m = new MaterialInfo();
            m.Texture.Should().BeAssignableTo<ITexture>();
            m.Ambient.Should().Be(0.1f);
            m.Diffuse.Should().Be(0.9f);
            m.Specular.Should().Be(0.9f);
            m.Shininess.Should().Be(200f);
        }

        [Fact]
        public void LightingWithPatternApplied()
        {
            var s = new Sphere();
            var m = new MaterialInfo
            {
                Texture = new StripeTexture(Colors.White, Colors.Black),
                Ambient = 1f,
                Diffuse = 0f,
                Specular = 0f
            };
            var eye = new Vector(0, 0, -1);
            var normal = new Normal(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), Colors.White);
            var c1 = PhongShading.Lighting(m, s, light, new Point(0.9f, 0, 0), eye, normal, 1);
            var c2 = PhongShading.Lighting(m, s, light, new Point(1.1f, 0, 0), eye, normal, 1);
            c1.Should().Be(Colors.White);
            c2.Should().Be(Colors.Black);
        }

        [Fact]
        public void DefaultReflectivityOfZero()
        {
            var m = new MaterialInfo();
            m.Reflective.Should().Be(0f);
        }

        [Fact]
        public void DefaultTransparencyOfZero()
        {
            var m = new MaterialInfo();
            m.Transparency.Should().Be(0f);
        }

        [Fact]
        public void DefaultRefractiveIndexOfOne()
        {
            var m = new MaterialInfo();
            m.RefractiveIndex.Should().Be(1f);
        }
    }
}