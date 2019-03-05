using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Octans
{
    public static class PPM
    {
        private const int MaxValue = 255;
        private const int MaxLineWidth = 70;

        public static string CanvasToPPM(Canvas c)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(
                              CultureInfo.InstalledUICulture,
                              @"P3
{0} {1}
{2}", c.Width, c.Height, MaxValue));
            AppendPixelData(c, sb, MaxLineWidth);
            return sb.ToString();
        }

        private static void AppendPixelData(Canvas c, StringBuilder sb, int maxLineWidth)
        {
            for (var j = 0; j < c.Height; j++)
            {
                var parts = new int[c.Width * 3];
                for (var i = 0; i < c.Width; i++)
                {
                    var (r, g, b) = ClampRgb(c.PixelAt(i, j));
                    var offset = i * 3;
                    parts[offset] = r;
                    parts[offset + 1] = g;
                    parts[offset + 2] = b;
                }

                foreach (var line in ColorValuesToStrings(parts, maxLineWidth))
                {
                    sb.AppendLine(line);
                }
            }
        }

        private static (int r, int g, int b) ClampRgb(Color c)
        {
            var c1 = c * MaxValue;
            return (
                r: Clamp((int) Math.Round(c1.Red), 0, MaxValue),
                g: Clamp((int) Math.Round(c1.Green), 0, MaxValue),
                b: Clamp((int) Math.Round(c1.Blue), 0, MaxValue));
        }

        private static IEnumerable<string> ColorValuesToStrings(IEnumerable<int> values, int maxLength)
        {
            var line = string.Join(' ', values);
            while (line.Length > 0)
            {
                if (line.Length <= maxLength)
                {
                    yield return line;
                    yield break;
                }

                var lastIndex = line.LastIndexOf(' ', maxLength, maxLength);
                yield return line.Substring(0, lastIndex);
                line = line.Substring(lastIndex + 1);
            }
        }

        private static int Clamp(int v, int min, int max) => Math.Min(max, Math.Max(min, v));
    }
}