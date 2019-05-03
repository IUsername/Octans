using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using static System.MathF;
using static Octans.Math;

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
       

        private static ReadOnlySpan<char> PixelDataSpan(int yStart, int yDelta,
            PixelVector resolution,
            ReadOnlySpan<float> lrgb,
            int maxLineWidth)
        {
            static int ToByte(float v) => Clamp(0, MaxValue, (int) (MaxValue * Utilities.GammaCorrect(v) + 0.5f));

            var k = yStart * resolution.X * 3;
            var sb = new StringBuilder();
            for (var j = yStart; j < resolution.Y && j < yStart + yDelta; ++j)
            {
                var parts = new int[resolution.X * 3];
                for (var i = 0; i < resolution.X; ++i)
                {
                    var offset = i * 3;
                    parts[offset] = ToByte(lrgb[k++]);
                    parts[offset + 1] = ToByte(lrgb[k++]);
                    parts[offset + 2] = ToByte(lrgb[k++]);
                }
                foreach (var line in ColorValuesToStrings(parts, maxLineWidth))
                {
                    sb.AppendLine(line);
                }
            }

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
                r: Clamp(0, MaxValue, (int) Round(c1.Red)),
                g: Clamp(0, MaxValue, (int) Round(c1.Green)),
                b: Clamp(0, MaxValue, (int) Round(c1.Blue)));
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

        public static void ToFile(Canvas c, string folderPath, string fileName)
        {
            // TODO: File streaming
            File.WriteAllText(Path.Combine(folderPath, fileName + ".ppm"), CanvasToPPM(c));
        }

        public static void ToFileStream(ReadOnlySpan<float> lrgb, PixelVector resolution, string folderPath, string fileName)
        {
            var path = Path.Combine(folderPath, fileName + ".ppm");
            using var output = new StreamWriter(path, false);
            output.Write(Preamble(resolution));
            const int yDelta = 1;
            for (var y =0; y < resolution.Y; y += yDelta)
            {
                output.Write(PixelDataSpan(y, yDelta, resolution, lrgb, MaxLineWidth));
            }
        }

        private static StringBuilder Preamble(PixelVector resolution)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format(
                              CultureInfo.InstalledUICulture,
                              @"P3
{0} {1}
{2}", resolution.X, resolution.Y, MaxValue));
            return sb;
        }
    }
}