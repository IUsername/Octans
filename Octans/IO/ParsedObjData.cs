using Octans.Geometry;

namespace Octans.IO
{
    public class ParsedObjData
    {
        public ParsedObjData(Point[] vertices, Normal[] normals, Group[] groups)
        {
            Vertices = vertices;
            Normals = normals;
            Groups = groups;
        }

        public Point[] Vertices { get; }

        public Normal[] Normals { get; }

        public Group DefaultGroup => Groups[0];

        public Group[] Groups { get; }

        public int VertexCount() => Vertices.Length - 1;
    }
}