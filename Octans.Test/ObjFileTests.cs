using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Octans.Test
{
    public class ObjFileTests
    {
        [Fact]
        public void IgnoreUnrecognizedLines()
        {
            var gibberish = @"
There was a young lady named Bright
who traveled much faster than light. 
She set out one day in a relative way,
and came back the previous night.";
            var data = ObjFile.Parse(gibberish);
            data.VertexCount().Should().Be(0);
        }

        [Fact]
        public void ParsesVertexRecords()
        {
            var file = @"
v -1 1 0
v -1.0000 0.5000 0.0000
v 1 0 0
v 1 1 0 
";
            var data = ObjFile.Parse(file);
            data.Vertices[1].Should().Be(new Point(-1, 1, 0));
            data.Vertices[2].Should().Be(new Point(-1, 0.5f, 0));
            data.Vertices[3].Should().Be(new Point(1, 0, 0));
            data.Vertices[4].Should().Be(new Point(1, 1, 0));
        }

        [Fact]
        public void ParsesNormalRecords()
        {
            var file = @"
vn 0 0 1
vn 0.707 0 -0.707
vn 1 2 3 
";
            var data = ObjFile.Parse(file);
            data.Normals[1].Should().Be(new Vector(0,0,1));
            data.Normals[2].Should().Be(new Vector(0.707f,0,-0.707f));
            data.Normals[3].Should().Be(new Vector(1, 2, 3));
        }

        [Fact]
        public void ParsesToTriangles()
        {
            var file = @"
v -1 1 0
v -1 0 0
v 1 0 0
v 1 1 0

f 1 2 3
f 1 3 4
";
            var data = ObjFile.Parse(file);
            var g = data.DefaultGroup;
            var t1 = (Triangle) g.Children[0];
            var t2 = (Triangle) g.Children[1];
            t1.P1.Should().Be(data.Vertices[1]);
            t1.P2.Should().Be(data.Vertices[2]);
            t1.P3.Should().Be(data.Vertices[3]);
            t2.P1.Should().Be(data.Vertices[1]);
            t2.P1.Should().Be(data.Vertices[1]);
            t2.P2.Should().Be(data.Vertices[3]);
            t2.P3.Should().Be(data.Vertices[4]);
        }

        [Fact]
        public void ParsesToTrianglesWithNormals()
        {
            var file = @"
v 0 1 0
v -1 0 0
v 1 0 0

vn -1 0 0
vn 1 0 0
vn 0 1 0

f 1//3 2//1 3//2
f 1/0/3 2/102/1 3/14/2
";
            var data = ObjFile.Parse(file);
            var g = data.DefaultGroup;
            var t1 = (SmoothTriangle)g.Children[0];
            var t2 = (SmoothTriangle)g.Children[1];
            t1.P1.Should().Be(data.Vertices[1]);
            t1.P2.Should().Be(data.Vertices[2]);
            t1.P3.Should().Be(data.Vertices[3]);
            t1.N1.Should().Be(data.Normals[3]);
            t1.N2.Should().Be(data.Normals[1]);
            t1.N3.Should().Be(data.Normals[2]);
        }

        [Fact]
        public void TriangulatesPolygons()
        {
            var file = @"
v -1 1 0 
v -1 0 0 
v 1 0 0 
v 1 1 0 
v 0 2 0

f 1 2 3 4 5
";
            var data = ObjFile.Parse(file);
            var g = data.DefaultGroup;
            var t1 = (Triangle) g.Children[0];
            var t2 = (Triangle) g.Children[1];
            var t3 = (Triangle) g.Children[2];
            t1.P1.Should().Be(data.Vertices[1]);
            t1.P2.Should().Be(data.Vertices[2]);
            t1.P3.Should().Be(data.Vertices[3]);
            t2.P1.Should().Be(data.Vertices[1]);
            t2.P2.Should().Be(data.Vertices[3]);
            t2.P3.Should().Be(data.Vertices[4]);
            t3.P1.Should().Be(data.Vertices[1]);
            t3.P2.Should().Be(data.Vertices[4]);
            t3.P3.Should().Be(data.Vertices[5]);
        }

        [Fact]
        public void CreatesMultipleGroups()
        {
            var file = @"
v -1 1 0 
v -1 0 0 
v 1 0 0 
v 1 1 0 
v 0 2 0

g FirstGroup
f 1 2 3
g SecondGroup
f 1 3 4
";
            var data = ObjFile.Parse(file);
            var g1 = data.Groups[0];
            var g2 = data.Groups[1];
            var t1 = (Triangle) g1.Children[0];
            var t2 = (Triangle) g2.Children[0];
            t1.P1.Should().Be(data.Vertices[1]);
            t1.P2.Should().Be(data.Vertices[2]);
            t1.P3.Should().Be(data.Vertices[3]);
            t2.P1.Should().Be(data.Vertices[1]);
            t2.P2.Should().Be(data.Vertices[3]);
            t2.P3.Should().Be(data.Vertices[4]);
        }

        [Fact(Skip = "Slow")]
        public void LoadFromFile()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("bin", x));
            var projectPath = string.Join(Path.DirectorySeparatorChar.ToString(),
                                          pathItems.Take(pathItems.Length - pos - 1));

            path = Path.Combine(projectPath, "teapot-low.obj");
            var data = ObjFile.ParseFile(path);
            var g = data.Groups[0];
            g.SetTransform(Transforms.Scale(0.1f).RotateX(-MathF.PI / 2f));

            var material = new Material
            {
                Pattern = new SolidColor(new Color(0.3f, 0.3f, 1f)),
                Reflective = 0.4f,
                Ambient = 0.2f,
                Diffuse = 0.3f
            };

            var floor = new Cube();
            floor.SetMaterial(material);
            var fg = new Group();
            fg.AddChild(floor);
            fg.SetTransform(Transforms.TranslateY(-1).Scale(1f));

            var w = new World();
            w.SetLights(new PointLight(new Point(-10, 10, -10), Colors.White));
            w.SetObjects(fg, g);

            var c = new Camera(300, 200, MathF.PI / 3f);
            c.SetTransform(Transforms.View(new Point(0, 1.5f, -5f), new Point(0, 1, 0), new Vector(0, 1, 0)));

            var canvas = c.Render(w);
            PPM.ToFile(canvas, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "teapot");
        }
    }
}