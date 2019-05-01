﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Octans.Geometry;
using Octans.Primitive;
using Pidgin;

namespace Octans.IO
{
    public class ObjFileMeshBuilder
    {
        private static readonly Parser<char, IObjPart> ObjPart = Parser.OneOf(Vertex, Normal, FaceIndices, Group);

        // TODO: This will require strings to be allocated and then tossed. Investigate direct conversion from character stream.
        private static Parser<char, float> FloatNum =>
            NumChar().AtLeastOnce().Select(cs => float.Parse(CreateString(cs)));

        private static Parser<char, Point> PointXYZ =>
            Parser.Map((x, y, z) => new Point(x, y, z), Tok(FloatNum), Tok(FloatNum), FloatNum).Labelled("PointXYZ");

        private static Parser<char, Normal> NormalXYZ =>
            Parser.Map((x, y, z) => new Normal(x, y, z), Tok(FloatNum), Tok(FloatNum), FloatNum).Labelled("NormalXYZ");

        private static Parser<char, IObjPart> Vertex =>
            Tok(Parser.String("v ")).Then(PointXYZ).Select<IObjPart>(p => new VertexPart(p)).Labelled("Vertex");

        private static Parser<char, IObjPart> Normal =>
            Tok(Parser.String("vn ")).Then(NormalXYZ).Select<IObjPart>(v => new NormalPart(v)).Labelled("Normal");

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

        public static ParsedMeshes Parse(string input)
        {
            ParsedMeshes data;
            using (var sr = new StringReader(input))
            {
                data = Parse(sr);
            }

            return data;
        }

        public static ParsedMeshes ParseFile(string path)
        {
            ParsedMeshes data;
            using (var sr = new StreamReader(path))
            {
                data = Parse(sr);
            }

            return data;
        }

        private static ParsedMeshes Parse(TextReader text)
        {
            // Vertex indices are 1-based.
            var vertices = new List<Point> {new Point()};
            var normals = new List<Normal> {new Normal()};
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
                            normals.Add(n.Normal);
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

            var pArray = vertices.ToArray();
            var nArray = normals.ToArray();
            var groupArray = groups.Select(og => og.BuildGroup(pArray, nArray)).ToArray();

            return new ParsedMeshes(groupArray,pArray, nArray);
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

            private string Name { get; }

            public void AddFaceIndices(FacePart f)
            {
                _faces.Add(f);
            }

            public GroupMeshData BuildGroup(Point[] vertices, Normal[] normals)
            {
                var indices = new List<int>();
                var faces = new List<int>();
                var n = new List<Normal>(normals);
                foreach (var f in _faces)
                {
                    for(var i=1; i<f.Indices.Length - 1; ++i )
                    {
                        indices.Add(f.Indices[0]);    
                        indices.Add(f.Indices[i]);    
                        indices.Add(f.Indices[i+1]);

                        faces.Add(f.Indices[0]);

                        if (f.Normals[0].HasValue && f.Normals[i].HasValue && f.Normals[i + 1].HasValue)
                        {
                            n[f.Indices[0]] = normals[f.Normals[0].Value];
                            n[f.Indices[i]] = normals[f.Normals[i].Value];
                            n[f.Indices[i+1]] = normals[f.Normals[i+1].Value];
                        }
                    }
                }

                return new GroupMeshData(Name, indices.ToArray(), vertices, n.ToArray(), faces.ToArray());
            }

            public bool IsDefined() => _faces.Count > 0;
        }

        private class NormalPart : IObjPart
        {
            public readonly Normal Normal;

            public NormalPart(Normal normal)
            {
                Normal = normal;
            }
        }

        public class GroupMeshData
        {
            private readonly int[] _vertexIndices;
            private readonly Point[] _p;
            private readonly Normal[] _n;
            private readonly int[] _faceIndices;
            public string Name { get; }

            public GroupMeshData(string name, int[] vertexIndices, Point[] p, Normal[] n, int[] faceIndices)
            {
                _vertexIndices = vertexIndices;
                _p = p;
                _n = n;
                _faceIndices = faceIndices;
                Name = name;
            }

            public IEnumerable<IShape> BuildShape(Transform objectToWorld, Transform worldToObject, bool reverseOrientation)
            {
                return TriangleMesh.CreateTriangleMesh(objectToWorld, worldToObject, reverseOrientation,
                                                       _vertexIndices.Length / 3,
                                                       _vertexIndices, _p.Length, _p, 
                                                       null, _n, null, null,
                                                       null, _faceIndices);
            }
        }

        public class ParsedMeshes
        {
            public IReadOnlyList<GroupMeshData> Meshes { get; }
            public Point[] Vertices { get; }
            public Normal[] Normals { get; }

            public ParsedMeshes(GroupMeshData[] meshes, Point[] vertices, Normal[] normals)
            {
                Meshes = meshes;
                Vertices = vertices;
                Normals = normals;
            }
        }
    }
}