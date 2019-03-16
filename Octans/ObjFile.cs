using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pidgin;

namespace Octans
{
    public class ObjFile
    {
        private static readonly Parser<char, IObjPart> ObjPart = Parser.OneOf(Vertex, FaceIndices, Group);

        // TODO: This will require strings to be allocated and then tossed. Investigate direct conversion from character stream.
        private static Parser<char, float> FloatNum =>
            NumChar().AtLeastOnce().Select(cs => float.Parse(CreateString(cs)));

        private static Parser<char, Point> PointXYZ =>
            Parser.Map((x, y, z) => new Point(x, y, z), Tok(FloatNum), Tok(FloatNum), FloatNum).Labelled("PointXYZ");

        private static Parser<char, IObjPart> Vertex =>
            Tok(Parser.Char('v')).Then(PointXYZ).Select<IObjPart>(p => new VertexPart(p)).Labelled("Vertex");

        private static Parser<char, IEnumerable<Maybe<int>>> IndexGroup =>
            Parser.Num.Optional().Separated(Parser.Char('/'));

        private static Parser<char, IEnumerable<IEnumerable<Maybe<int>>>> FaceIndexGroup =>
            IndexGroup.Separated(Parser.Whitespace);

        private static Parser<char, IObjPart> FaceIndices =>
            Tok(Parser.Char('f')).Then(FaceIndexGroup).Select<IObjPart>(FacePart.Extract).Labelled("Face");

        private static Parser<char, string> Identifier =>
            Parser<char>.Token(char.IsLetterOrDigit).AtLeastOnceString();

        private static Parser<char, IObjPart> Group =>
            Tok(Parser.Char('g')).Then(Identifier).Select<IObjPart>(n => new GroupPart(n));

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
                        case FacePart f:
                            current.AddFaceIndices(f.Indices);
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

            var groupArray = groups.Select(og => og.BuildGroup(vertices)).ToArray();

            return new ParsedObjData(vertices.ToArray(), groupArray);
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

            private FacePart(int[] indices)
            {
                Indices = indices;
            }

            public static FacePart Extract(IEnumerable<IEnumerable<Maybe<int>>> indices)
            {
                return new FacePart(indices.Select(indexGroup => indexGroup.First().Value).ToArray());
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
            private readonly IList<int[]> _faces = new List<int[]>();

            public ObjGroup(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public void AddFaceIndices(int[] indices)
            {
                _faces.Add(indices);
            }

            public Group BuildGroup(IReadOnlyList<Point> vertices)
            {
                var g = new Group();
                foreach (var f in _faces)
                {
                    for (var i = 1; i < f.Length - 1; i++)
                    {
                        var p1 = vertices[f[0]];
                        var p2 = vertices[f[i]];
                        var p3 = vertices[f[i + 1]];
                        var t = new Triangle(p1, p2, p3);
                        g.AddChild(t);
                    }
                }

                return g;
            }

            public bool IsDefined() => _faces.Count > 0;
        }
    }

    public class ParsedObjData
    {
        public ParsedObjData(Point[] vertices, Group[] groups)
        {
            Vertices = vertices;
            Groups = groups;
        }

        public Point[] Vertices { get; }

        public Group DefaultGroup => Groups[0];

        public Group[] Groups { get; }

        public int VertexCount() => Vertices.Length - 1;
    }
}