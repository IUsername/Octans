using System;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ShadingTests
    {
        [Fact]
        public void EyeBetweenLightAndSurface()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, false);
            result.Should().BeEquivalentTo(new Color(1.9f, 1.9f, 1.9f));
        }

        [Fact]
        public void EyeOffset45Deg()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, MathF.Sqrt(2) / 2, -MathF.Sqrt(2) / 2);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, false);
            result.Should().BeEquivalentTo(new Color(1.0f, 1.0f, 1.0f));
        }

        [Fact]
        public void LightOffset45Deg()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1f);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 10, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, false);
            result.Should().BeEquivalentTo(new Color(0.7364f, 0.7364f, 0.7364f));
        }

        [Fact]
        public void FullReflectionOfLightOffset45Deg()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, -MathF.Sqrt(2) / 2, -MathF.Sqrt(2) / 2);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 10, -10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, false);
            result.Should().BeEquivalentTo(new Color(1.6364f, 1.6364f, 1.6364f));
        }

        [Fact]
        public void LightBehindSurface()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1f);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, 10), new Color(1f, 1f, 1f));
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, false);
            result.Should().BeEquivalentTo(new Color(0.1f, 0.1f, 0.1f));
        }

        [Fact]
        public void LightOnSurfaceInShadow()
        {
            var s = new Sphere();
            var m = new Material();
            var position = Point.Zero;
            var eyeV = new Vector(0, 0, -1f);
            var normalV = new Vector(0, 0, -1);
            var light = new PointLight(new Point(0, 0, -10), new Color(1f, 1f, 1f));
            const bool inShadow = true;
            var result = Shading.Lighting(m, s, light, position, eyeV, normalV, inShadow);
            result.Should().BeEquivalentTo(new Color(0.1f, 0.1f, 0.1f));
        }

        [Fact]
        public void ShadingOutsideIntersection()
        {
            var w = World.Default();
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = w.Objects[0];
            var i = new Intersection(4f, shape);
            var comps = new IntersectionInfo(i, r);
            var c = Shading.HitColor(w, comps);
            c.Should().Be(new Color(0.38066f, 0.47583f, 0.2855f));
        }

        [Fact]
        public void ShadingInsideIntersection()
        {
            var w = World.Default();
            w.SetLights(new PointLight(new Point(0, 0.25f, 0), new Color(1f, 1f, 1f)));
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var shape = w.Objects[1];
            var i = new Intersection(0.5f, shape);
            var comps = new IntersectionInfo(i, r);
            var c = Shading.HitColor(w, comps);
            c.Should().Be(new Color(0.90498f, 0.90498f, 0.90498f));
        }

        [Fact]
        public void ColorWhenRayMissesIsBlack()
        {
            var w = World.Default();
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 1, 0));
            var c = Shading.ColorAt(w, r);
            c.Should().Be(new Color(0, 0, 0));
        }

        [Fact]
        public void ColorWhenRayHits()
        {
            var w = World.Default();
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var c = Shading.ColorAt(w, r);
            c.Should().Be(new Color(0.38066f, 0.47583f, 0.2855f));
        }

        [Fact]
        public void ColorWithIntersectionBehind()
        {
            var w = World.Default();
            w.Objects[0].Material.Ambient = 1f;
            w.Objects[1].Material.Ambient = 1f;
            var r = new Ray(new Point(0, 0, 0.75f), new Vector(0, 0, -1));
            var c = Shading.ColorAt(w, r);
            c.Should().Be(w.Objects[1].Material.Pattern.LocalColorAt(Point.Zero));
        }

        [Fact]
        public void NotInShadowWhenNothingBetweenLightAndPoint()
        {
            var w = World.Default();
            var p = new Point(0, 10, 0);
            Shading.IsShadowed(w, p).Should().BeFalse();
        }

        [Fact]
        public void InShadowWhenAnObjectIsBetweenLightAndPoint()
        {
            var w = World.Default();
            var p = new Point(10, -10, 10);
            Shading.IsShadowed(w, p).Should().BeTrue();
        }

        [Fact]
        public void NotInShadowWhenObjectIsBehindPoint()
        {
            var w = World.Default();
            var p = new Point(-2, 2, -2);
            Shading.IsShadowed(w, p).Should().BeFalse();
        }
    }
}