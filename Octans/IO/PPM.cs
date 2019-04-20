using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Octans.IO
{
    public static partial class PPM
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

        public static string LRGBToPPM(ReadOnlySpan<float> lrgb, PixelVector resolution)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(
                              CultureInfo.InstalledUICulture,
                              @"P3
{0} {1}
{2}", resolution.X, resolution.Y, MaxValue));
            AppendPixelData(resolution, lrgb, sb, MaxLineWidth);
            return sb.ToString();
        }

        private static void AppendPixelData(PixelVector resolution, ReadOnlySpan<float> lrgb, StringBuilder sb, int maxLineWidth)
        {
            var k = 0;
            for (var j = 0; j < resolution.Y; ++j)
            {
                var parts = new int[resolution.X * 3];
                for (var i = 0; i < resolution.X; ++i)
                {
                    var offset = i * 3;
                    parts[offset] = Math.Clamp((int)(lrgb[k++] * 100), 0, MaxValue);
                    parts[offset+1] = Math.Clamp((int)(lrgb[k++] * 100), 0, MaxValue);
                    parts[offset+2] = Math.Clamp((int)(lrgb[k++] * 100), 0, MaxValue);

                }
                foreach (var line in ColorValuesToStrings(parts, maxLineWidth))
                {
                    sb.AppendLine(line);
                }
            }
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
                r: Clamp((int) System.MathF.Round(c1.Red), 0, MaxValue),
                g: Clamp((int) System.MathF.Round(c1.Green), 0, MaxValue),
                b: Clamp((int) System.MathF.Round(c1.Blue), 0, MaxValue));
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

        // TODO: Move to MathFunctions
        private static int Clamp(int v, int min, int max) => Math.Min(max, Math.Max(min, v));

        public static void ToFile(Canvas c, string folderPath, string fileName)
        {
            // TODO: File streaming
            File.WriteAllText(Path.Combine(folderPath, fileName + ".ppm"), CanvasToPPM(c));
        }

        public static void ToFile(ReadOnlySpan<float> lrgb,  PixelVector resolution, string folderPath, string fileName)
        {
            // TODO: File streaming
            File.WriteAllText(Path.Combine(folderPath, fileName + ".ppm"), LRGBToPPM(lrgb, resolution));
        }
    }
}