using System;
using System.IO;
using System.Linq;

namespace Octans.ConsoleApp
{
    internal static class TestScenes
    {
        private static string GetExecutionPath()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = path.Split(Path.DirectorySeparatorChar);
            var pos = pathItems.Reverse().ToList().FindIndex(d => string.Equals("bin", d));
            return string.Join(Path.DirectorySeparatorChar.ToString(),
                               pathItems.Take(pathItems.Length - pos - 1));
        }
    }
}