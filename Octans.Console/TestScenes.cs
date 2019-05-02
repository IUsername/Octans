using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Octans.Primitive;

namespace Octans.ConsoleApp
{
    internal static class TestScenes
    {
        public static string GetExecutionPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(d => string.Equals("bin", d));
            return string.Join(Path.DirectorySeparatorChar.ToString(),
                               pathItems.Take(pathItems.Length - pos - 1));
        }

        public static IEnumerable<IShape> CreatePlane(Point min, Point max)
        {
            var tr = Transform.Identity;
            return TriangleMesh.CreateTriangleMesh(
                tr,
                Transform.Invert(tr),
                false,
                2,
                new[] { 0, 1, 2, 0, 2, 3 },
                4,
                new[]
                {
                    min,
                    new Point(min.X, max.Y, max.Z),
                    max,
                    new Point(max.X, min.Y, min.Z), 
                },
                null,
                null, null, null, null, null);
        }
    }
}