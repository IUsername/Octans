using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Octans.Geometry;
using Pidgin;

namespace Octans
{
    public class ObjFile
    {
        private static readonly Parser<char, IObjPart> ObjPart = Parser.OneOf(Vertex, Normal, FaceIndices, Group);

        // TODO: This will require strings to be allocated and then tossed. Investigate direct conversion from character stream.
        private static Parser<char, float> FloatNum =>
            NumChar().AtLeastOnce().Select(cs => float.Parse(CreateString(cs)));

        private static Parser<char, Point> PointXYZ =>
            Parser.Map((x, y, z) => new Point(x, y, z), Tok(FloatNum), Tok(FloatNum), FloatNum).Labelled("PointXYZ");

        private static Parser<char, Vector> VectorXYZ =>
            Parser.Map((x, y, z) => new Vector(x, y, z), Tok(FloatNum), Tok(FloatNum), FloatNum).Labelled("VectorXYZ");

        private static Parser<char, IObjPart> Vertex =>
            Tok(Parser.String("v ")).Then(PointXYZ).Select<IObjPart>(p => new VertexPart(p)).Labelled("Vertex");

        private static Parser<char, IObjPart> Normal =>
            Tok(Parser.String("vn ")).Then(VectorXYZ).Select<IObjPart>(v => new NormalPart(v)).Labelled("Normal");

        private static Parser<char, IEnumerable<Maybe<int>>> IndexGroup =>
            Parser.Num.Optional().Separated(Parser.Char('/'));

        private static Parser<char, IEnumerable<IEnumerable<Maybe<int>>>> FaceIndexGroup =>
            IndexGroup.Separated(Parser.Whitespace);

        private static Parser<char, IObjPart> FaceIndices =>
            Tok(Parser.String("f ")).Then(FaceIndexGroup).Select<IObjPart>(FacePart.Extract).Labelled("Face");

        private static Parser<char, string> Identifier =>
            Parser<char>.Token(char.IsLetterOrDigit).AtLeastOnceString();

        private static Parser<char, IObjPart> Group =>
            Tok(Parser.String("g ")).Then(Identifier).Select<IObjPart>(n => new GroupPart(n));

        public static ParsedObjData Parse(string input)
        {
            ParsedObjData data;
            using (var sr = new StringReader(input))
            {
                data = Parse(sr);
            }

            return data;
        }

        public static ParsedObjData ParseFile(string path)
        {
            ParsedObjData data;
            using (var sr = new StreamReader(path))
            {
                data = Parse(sr);
            }

            return data;
        }

        private static ParsedObjData Parse(TextReader text)
        {
            // Vertex indices are 1-based.
            var vertices = new List<Point> {new Point()};
            var normals = new List<Vector> {new Vector()};
            var groups = new List<ObjGroup>();
            var current = new ObjGroup("Default");
            string line;
            while ((line = text.ReadLine()) != null)
            {
                try
                {
                    var part = ObjPart.ParseOrThrow(line.Trim());
                    switch (part)
                    {
                        case VertexPart v:
                            vertices.Add(v.Point);
                            break;
                        case NormalPart n:
                            normals.Add(n.Vector);
                            break;
                        case FacePart f:
                            current.AddFaceIndices(f);
                            break;
                        case GroupPart g:
                            if (current.IsDefined())
                            {
                                groups.Add(current);
                            }

                            current = new ObjGroup(g.Name);
                            break;
                    }
                }
                catch (ParseException)
                {
                }
            }

            if (current.IsDefined())
            {
                groups.Add(current);
            }

            var groupArray = groups.Select(og => og.BuildGroup(vertices, normals)).ToArray();

            return new ParsedObjData(vertices.ToArray(), normals.ToArray(), groupArray);
        }

        private static Parser<char, T> Tok<T>(Parser<char, T> token) =>
            Parser.Try(token).Before(Parser.SkipWhitespaces);

        private static bool IsValidNumChar(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case '-':
                case '+':
                    return true;
                default: return false;
            }
        }

        private static Parser<char, char> NumChar() => Parser<char>.Token(IsValidNumChar);

        private static string CreateString(IEnumerable<char> chars)
        {
            var sb = new StringBuilder();
            foreach (var c in chars)
            {
                sb.Append(c);
            }

            return sb.ToString();
        }

        private interface IObjPart
        {
        }

        private class VertexPart : IObjPart
        {
            public readonly Point Point;

            public VertexPart(Point point)
            {
                Point = point;
            }
        }

        private class FacePart : IObjPart
        {
            public readonly int[] Indices;
            public readonly int?[] Normals;

            private FacePart(int[] indices, int?[] normals)
            {
                Normals = normals;
                Indices = indices;
            }

            public static FacePart Extract(IEnumerable<IEnumerable<Maybe<int>>> indices)
            {
                var groups = indices as IEnumerable<Maybe<int>>[] ?? indices.ToArray();
                var ind = groups.Select(indexGroup => indexGroup.First().Value).ToArray();
                var nor = groups.Select(indexGroup => indexGroup.Skip(2).FirstOrDefault())
                                .Select(m => m.HasValue ? m.Value : new int?())
                                .ToArray();
                return new FacePart(ind, nor);
            }
        }

        private class GroupPart : IObjPart
        {
            public GroupPart(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private class ObjGroup
        {
            private readonly IList<FacePart> _faces = new List<FacePart>();

            public ObjGroup(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public void AddFaceIndices(FacePart f)
            {
                _faces.Add(f);
            }

            public Group BuildGroup(IReadOnlyList<Point> vertices, IReadOnlyList<Vector> normals)
            {
                var g = new Group();
                foreach (var f in _faces)
                {
                    for (var i = 1; i < f.Indices.Length - 1; i++)
                    {
                        var p1 = vertices[f.Indices[0]];
                        var p2 = vertices[f.Indices[i]];
                        var p3 = vertices[f.Indices[i + 1]];
                        IGeometry t;
                        if (f.Normals[0].HasValue && f.Normals[i].HasValue && f.Normals[i + 1].HasValue)
                        {
                            var n1 = normals[f.Normals[0].Value];
                            var n2 = normals[f.Normals[i].Value];
                            // ReSharper disable once PossibleInvalidOperationException
                            var n3 = normals[f.Normals[i + 1].Value];
                            t = new SmoothTriangle(p1, p2, p3, n1, n2, n3);
                        }
                        else
                        {
                            t = new Triangle(p1, p2, p3);
                        }

                        g.AddChild(t);
                    }
                }

                return g;
            }

            public bool IsDefined() => _faces.Count > 0;
        }

        private class NormalPart : IObjPart
        {
            public readonly Vector Vector;

            public NormalPart(Vector vector)
            {
                Vector = vector;
            }
        }
    }

    public class ParsedObjData
    {
        public ParsedObjData(Point[] vertices, Vector[] normals, Group[] groups)
        {
            Vertices = vertices;
            Normals = normals;
            Groups = groups;
        }

        public Point[] Vertices { get; }

        public Vector[] Normals { get; }

        public Group DefaultGroup => Groups[0];

        public Group[] Groups { get; }

        public int VertexCount() => Vertices.Length - 1;
    }
}