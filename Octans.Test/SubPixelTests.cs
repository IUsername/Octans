using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class SubPixelTests
    {
        [Fact]
        public void SubPixelAlignToLowestDivisions()
        {
            var sp = SubPixel.Create(1, 1, 1, 1, 1);
            sp.X.Should().Be(2);
            sp.Y.Should().Be(2);
            sp.Divisions.Should().Be(1);
            sp.Dx.Should().Be(0);
            sp.Dy.Should().Be(0);

            sp = SubPixel.Create(1, 1, 2, 1, 1);
            sp.X.Should().Be(1);
            sp.Y.Should().Be(1);
            sp.Divisions.Should().Be(2);
            sp.Dx.Should().Be(1);
            sp.Dy.Should().Be(1);

            sp = SubPixel.Create(1, 3, 4, 3, 4);
            sp.X.Should().Be(1);
            sp.Y.Should().Be(4);
            sp.Divisions.Should().Be(4);
            sp.Dx.Should().Be(3);
            sp.Dy.Should().Be(0);
        }

        [Fact]
        public void CanFindSubPixelCorners()
        {
            var sp = SubPixel.Create(0, 0, 2, 1, 1);
            var (tl, tr, bl, br) = SubPixel.Corners(in sp);
            tl.Should().Be(SubPixel.Create(0, 0, 1, 0, 0));
            tr.Should().Be(SubPixel.Create(1, 0, 1, 0, 0));
            bl.Should().Be(SubPixel.Create(0, 1, 1, 0, 0));
            br.Should().Be(SubPixel.Create(1, 1, 1, 0, 0));

            sp = SubPixel.Create(3, 4, 4, 3, 2);
            (tl, tr, bl, br) = SubPixel.Corners(in sp);
            tl.Should().Be(SubPixel.Create(3, 4, 4, 2, 1));
            tr.Should().Be(SubPixel.Create(4, 4, 4, 0, 1));
            bl.Should().Be(SubPixel.Create(3, 4, 4, 2, 3));
            br.Should().Be(SubPixel.Create(4, 4, 4, 0, 3));
        }

        [Fact]
        public void FindsCenterFromTwoSubPixels()
        {
            var tl = SubPixel.Create(0, 0, 1, 0, 0);
            var br = SubPixel.Create(1, 1, 1, 0, 0);
            var c = SubPixel.Center(in tl, in br);
            c.Should().Be(SubPixel.Create(0, 0, 2, 1, 1));

            tl = SubPixel.Create(2, 3, 2, 1, 1);
            br = SubPixel.Create(3, 4, 1, 0, 0);
            c = SubPixel.Center(in tl, in br);
            c.Should().Be(SubPixel.Create(2, 3, 4, 3, 3));
        }
    }
}