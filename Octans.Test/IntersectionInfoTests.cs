using System;
using System.Collections.Generic;
using FluentAssertions;
using Octans.Geometry;
using Octans.Test.Geometry;
using Xunit;

namespace Octans.Test
{
    public class IntersectionInfoTests
    {
        [Fact]
        public void ContainsPrecomputedData()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(4, shape);
            var comps = new IntersectionInfo(i, r);
            comps.T.Should().Be(4f);
            comps.Geometry.Should().Be(shape);
            comps.Point.Should().Be(new Point(0, 0, -1));
            comps.Eye.Should().Be(new Vector(0, 0, -1));
            comps.Normal.Should().Be(new Normal(0, 0, -1));
        }

        [Fact]
        public void IsNotInsideWhenIntersectsOutsideSurface()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(4, shape);
            var comps = new IntersectionInfo(i, r);
            comps.IsInside.Should().BeFalse();
        }

        [Fact]
        public void IsInsideWhenIntersectsInsideSurface()
        {
            var r = new Ray(new Point(0, 0, 0), new Vector(0, 0, 1));
            var shape = new Sphere();
            var i = new Intersection(1, shape);
            var comps = new IntersectionInfo(i, r);
            comps.IsInside.Should().BeTrue();
            comps.Normal.Should().Be(new Normal(0, 0, -1));
        }

        [Fact]
        public void DeterminesOverPoint()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            shape.SetTransform(Transforms.Translate(0, 0, 1));
            var i = new Intersection(5, shape);
            var comps = new IntersectionInfo(i, r);
            comps.OverPoint.Z.Should().BeLessThan(-IntersectionInfo.Epsilon / 2f);
            comps.Point.Z.Should().BeGreaterThan(comps.OverPoint.Z);
        }

        [Fact]
        public void ComputesReflectionVector()
        {
            var shape = new Plane();
            var r = new Ray(new Point(0, 1, -1), new Vector(0, -MathF.Sqrt(2f) / 2f, MathF.Sqrt(2f) / 2f));
            var i = new Intersection(MathF.Sqrt(2f), shape);
            var comps = new IntersectionInfo(i, r);
            comps.Reflect.Should().Be(new Vector(0, MathF.Sqrt(2f) / 2f, MathF.Sqrt(2f) / 2f));
        }

        [Fact]
        public void DeterminesN1AndN2()
        {
            var a = Spheres.GlassSphere();
            a.SetTransform(Transforms.Scale(2f));
            a.Material.RefractiveIndex = 1.5f;

            var b = Spheres.GlassSphere();
            b.SetTransform(Transforms.TranslateZ(-0.25f));
            b.Material.RefractiveIndex = 2.0f;

            var c = Spheres.GlassSphere();
            c.SetTransform(Transforms.TranslateZ(0.25f));
            c.Material.RefractiveIndex = 2.5f;

            var r = new Ray(new Point(0, 0, -4), new Vector(0, 0, 1));
            var xs = Intersections.Create(
                new Intersection(2.00f, a),
                new Intersection(2.75f, b),
                new Intersection(3.25f, c),
                new Intersection(4.75f, b),
                new Intersection(5.25f, c),
                new Intersection(6.00f, a)
            );

            var comps = new List<IntersectionInfo>();
            for(var i=0; i<xs.Count; i++)
            {
                comps.Add(new IntersectionInfo(xs[i], r, xs));
            }

            comps[0].N1.Should().Be(1.0f);
            comps[0].N2.Should().Be(1.5f);
            comps[1].N1.Should().Be(1.5f);
            comps[1].N2.Should().Be(2.0f);
            comps[2].N1.Should().Be(2.0f);
            comps[2].N2.Should().Be(2.5f);
            comps[3].N1.Should().Be(2.5f);
            comps[3].N2.Should().Be(2.5f);
            comps[4].N1.Should().Be(2.5f);
            comps[4].N2.Should().Be(1.5f);
            comps[5].N1.Should().Be(1.5f);
            comps[5].N2.Should().Be(1.0f);
        }

        [Fact]
        public void DeterminesUnderPoint()
        {
            var r = new Ray(new Point(0, 0, -5), new Vector(0, 0, 1));
            var shape = new Sphere();
            shape.SetTransform(Transforms.Translate(0, 0, 1));
            var i = new Intersection(5, shape);
            var comps = new IntersectionInfo(i, r);
            comps.UnderPoint.Z.Should().BeGreaterThan(IntersectionInfo.Epsilon / 2f);
            comps.Point.Z.Should().BeLessThan(comps.UnderPoint.Z);
        }
    }
}