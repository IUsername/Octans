using System;
using FluentAssertions;
using Octans.Texture;
using Xunit;

namespace Octans.Test.Texture
{
    public class UVMappingTests
    {
        [Fact]
        public void SphericalMap()
        {
            UVMapping.Spherical(new Point(0, 0, -1)).Should().Be(new UVPoint(0.0f, 0.5f));
            UVMapping.Spherical(new Point(1, 0, 0)).Should().Be(new UVPoint(0.25f, 0.5f));
            UVMapping.Spherical(new Point(0, 0, 1)).Should().Be(new UVPoint(0.5f, 0.5f));
            UVMapping.Spherical(new Point(-1, 0, 0)).Should().Be(new UVPoint(0.75f, 0.5f));
            UVMapping.Spherical(new Point(0, 1, 0)).Should().Be(new UVPoint(0.5f, 1.0f));
            UVMapping.Spherical(new Point(0, -1, 0)).Should().Be(new UVPoint(0.5f, 0.0f));
            var uv = UVMapping.Spherical(new Point(MathF.Sqrt(2f) / 2f, MathF.Sqrt(2f) / 2f, 0));
            uv.U.Should().BeApproximately(0.25f, 0.00001f);
            uv.V.Should().BeApproximately(0.75f, 0.00001f);
        }

        [Fact]
        public void PlanarMap()
        {
            UVMapping.Planar(new Point(0.25f, 0, 0.5f)).Should().Be(new UVPoint(0.25f, 0.5f));
            UVMapping.Planar(new Point(0.25f, 0, -0.25f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Planar(new Point(0.25f, 0.5f, -0.25f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Planar(new Point(1.25f, 0f, 0.5f)).Should().Be(new UVPoint(0.25f, 0.5f));
            UVMapping.Planar(new Point(0.25f, 0f, -1.75f)).Should().Be(new UVPoint(0.25f, 0.25f));
            UVMapping.Planar(new Point(1f, 0f, -1f)).Should().Be(new UVPoint(0.0f, 0.0f));
            UVMapping.Planar(new Point(0f, 0f, 0f)).Should().Be(new UVPoint(0.0f, 0.0f));
        }

        [Fact]
        public void CylindricalMap()
        {
            UVMapping.Cylindrical(new Point(0f, 0, -1f)).Should().Be(new UVPoint(0.0f, 0.0f));
            UVMapping.Cylindrical(new Point(0f, 0.5f, -1f)).Should().Be(new UVPoint(0.0f, 0.5f));
            UVMapping.Cylindrical(new Point(0f, 1f, -1f)).Should().Be(new UVPoint(0.0f, 0.0f));
            UVMapping.Cylindrical(new Point(0.70711f, 0.5f, -0.70711f)).Should().Be(new UVPoint(0.125f, 0.5f));
            UVMapping.Cylindrical(new Point(1f, 0.5f, 0f)).Should().Be(new UVPoint(0.25f, 0.5f));
            UVMapping.Cylindrical(new Point(0.70711f, 0.5f, 0.70711f)).Should().Be(new UVPoint(0.375f, 0.5f));
            UVMapping.Cylindrical(new Point(0.0f, -0.25f, 1f)).Should().Be(new UVPoint(0.5f, 0.75f));
            UVMapping.Cylindrical(new Point(-0.70711f, 0.5f, 0.70711f)).Should().Be(new UVPoint(0.625f, 0.5f));
            UVMapping.Cylindrical(new Point(-1f, 1.25f, 0f)).Should().Be(new UVPoint(0.75f, 0.25f));
            UVMapping.Cylindrical(new Point(-0.70711f, 0.5f, -0.70711f)).Should().Be(new UVPoint(0.875f, 0.5f));
        }

        [Fact]
        public void IdentifyCubeFaceFromPoint()
        {
            UVMapping.PointToCubeFace(new Point(-1, 0.5f, -0.25f)).Should().Be(UVMapping.CubeFace.Left);
            UVMapping.PointToCubeFace(new Point(1.1f, -0.75f, -0.8f)).Should().Be(UVMapping.CubeFace.Right);
            UVMapping.PointToCubeFace(new Point(0.1f, 0.6f, 0.9f)).Should().Be(UVMapping.CubeFace.Front);
            UVMapping.PointToCubeFace(new Point(-0.7f, 0f, -2f)).Should().Be(UVMapping.CubeFace.Back);
            UVMapping.PointToCubeFace(new Point(0.5f, 1f, 0.9f)).Should().Be(UVMapping.CubeFace.Top);
            UVMapping.PointToCubeFace(new Point(-0.2f, -1.3f, 1.1f)).Should().Be(UVMapping.CubeFace.Bottom);
        }

        [Fact]
        public void CubicalMap()
        {
            // Front
            UVMapping.Cubical(new Point(-0.5f, 0.5f, 1)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(0.5f, -0.5f, 1)).Should().Be(new UVPoint(0.75f, 0.25f));
            // Back
            UVMapping.Cubical(new Point(0.5f, 0.5f, -1)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(-0.5f, -0.5f, -1)).Should().Be(new UVPoint(0.75f, 0.25f));
            // Left
            UVMapping.Cubical(new Point(-1f, 0.5f, -0.5f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(-1f, -0.5f, 0.5f)).Should().Be(new UVPoint(0.75f, 0.25f));
            // Right
            UVMapping.Cubical(new Point(1f, 0.5f, 0.5f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(1f, -0.5f, -0.5f)).Should().Be(new UVPoint(0.75f, 0.25f));
            // Top
            UVMapping.Cubical(new Point(-0.5f, 1f, -0.5f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(0.5f, 1f, 0.5f)).Should().Be(new UVPoint(0.75f, 0.25f));
            // Bottom
            UVMapping.Cubical(new Point(-0.5f, -1f, 0.5f)).Should().Be(new UVPoint(0.25f, 0.75f));
            UVMapping.Cubical(new Point(0.5f, -1f, -0.5f)).Should().Be(new UVPoint(0.75f, 0.25f));
        }

        [Fact]
        public void SkyBoxMap()
        {
            // Front
            var uv = UVMapping.SkyBox(new Point(-0.5f, 0.5f, 1));
            uv.U.Should().BeApproximately(5f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(7f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(0.5f, -0.5f, 1));
            uv.U.Should().BeApproximately(7f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(5f / 12f, 0.0001f);
            // Back
            uv = UVMapping.SkyBox(new Point(0.5f, 0.5f, -1));
            uv.U.Should().BeApproximately(13f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(7f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(-0.5f, -0.5f, -1));
            uv.U.Should().BeApproximately(15f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(5f / 12f, 0.0001f);
            // Left
            uv = UVMapping.SkyBox(new Point(-1f, 0.5f, -0.5f));
            uv.U.Should().BeApproximately(1f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(7f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(-1f, -0.5f, 0.5f));
            uv.U.Should().BeApproximately(3f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(5f / 12f, 0.0001f);
            // Right
            uv = UVMapping.SkyBox(new Point(1f, 0.5f, 0.5f));
            uv.U.Should().BeApproximately(9f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(7f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(1f, -0.5f, -0.5f));
            uv.U.Should().BeApproximately(11f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(5f / 12f, 0.0001f);
            // Top
            uv = UVMapping.SkyBox(new Point(-0.5f, 1f, -0.5f));
            uv.U.Should().BeApproximately(5f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(11f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(0.5f, 1f, 0.5f));
            uv.U.Should().BeApproximately(7f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(9f / 12f, 0.0001f);
            // Bottom
            uv = UVMapping.SkyBox(new Point(-0.5f, -1f, 0.5f));
            uv.U.Should().BeApproximately(5f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(3f / 12f, 0.0001f);
            uv = UVMapping.SkyBox(new Point(0.5f, -1f, -0.5f));
            uv.U.Should().BeApproximately(7f / 16f, 0.0001f);
            uv.V.Should().BeApproximately(1f / 12f, 0.0001f);
        }
    }
}