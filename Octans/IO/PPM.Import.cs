using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pidgin;

namespace Octans.IO
{
    public static partial class PPM
    {
        public static Canvas Parse(string input)
        {
            Canvas data;
            using (var sr = new StringReader(input))
            {
                data = Parse(sr);
            }

            return data;
        }

        public static Canvas ParseFile(string path)
        {
            Canvas data;
            using (var sr = new StreamReader(path))
            {
                data = Parse(sr);
            }

            return data;
        }

        private static Parser<char, IPPMPart> FileType =>
            Parser.Char('P').Then(Tok(Parser.Num)).Select(i => (IPPMPart) new FileTypePart(i)).Labelled("File type");

        private static Parser<char, IPPMPart> ImageSize =>
            Parser.Map((x, y) => (IPPMPart) new ImageSizePart(x, y), Tok(Parser.Num), Tok(Parser.Num))
                  .Labelled("Image size");

        private static Parser<char, IPPMPart> Scale =>
            Tok(Parser.Num).Select(s => (IPPMPart) new ScalePart(s)).Labelled("Scale");

        //private static Parser<char, Tuple<int, int, int>> Color =>
        //    Parser.Map((r, g, b) => new Tuple<int, int, int>(r, g, b), Tok(Parser.Num), Tok(Parser.Num),
        //               Tok(Parser.Num))
        //          .Labelled("Color RGB");

        private static Parser<char, int> ColorFragment => Tok(Parser.Num).Labelled("Color fragment");

        private static Parser<char, IPPMPart> PixelData =>
            Tok(ColorFragment).Many().Select(c => (IPPMPart)new PixelDataPart(c)).Labelled("Pixel data");

        private static Parser<char, T> Tok<T>(Parser<char, T> token) =>
            Parser.Try(token).Before(Parser.SkipWhitespaces);

        private static Canvas Parse(TextReader text)
        {
            var parsers = new List<Parser<char, IPPMPart>>(new[] {FileType, ImageSize, Scale, PixelData});
            string line;
            var index = 0;
            var current = parsers[index];
            var builder = new CanvasBuilder(3);
            bool complete = false;
            while ((line = text.ReadLine()) != null)
            {
                if (complete) break;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith('#'))
                {
                    // Skip comment lines;
                    continue;
                }

                var parsed = current.ParseOrThrow(line);
                var result = parsed.Process(builder);
                switch (result)
                {
                    case ParseResult.NextParser:
                        current = parsers[++index];
                        break;
                    case ParseResult.Complete:
                        complete = true;
                        break;
                    case ParseResult.Failure:
                        var error = parsed.GetLastFailureMessage();
                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new FileLoadException(error);
                        }

                        throw new FileLoadException();
                    case ParseResult.Continue:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return builder.GetCanvas();
        }

        private class CanvasBuilder
        {
            private readonly int _supportedFileType;
            private Canvas _canvas;

            private int _currentX;
            private int _currentY;
            private readonly Queue<int> _colorFragments = new Queue<int>();

            public CanvasBuilder(int supportedFileType)
            {
                _supportedFileType = supportedFileType;
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
            public int FileType { get; private set; }
            public int Scale { get; private set; }

            public void SetImageSize(int x, int y)
            {
                Width = x;
                Height = y;

                _canvas = new Canvas(Width, Height);
            }

            public void SetFileType(int fileType)
            {
                if (fileType != _supportedFileType)
                {
                    throw new InvalidOperationException($"Only supports file type {_supportedFileType}");
                }

                FileType = fileType;
            }

            public void SetScale(int scale)
            {
                Scale = scale;
            }

            public bool AddPixel(in Color color)
            {
                if (_currentY >= Height)
                {
                    return false;
                }

                _canvas.WritePixel(in color, _currentX++, _currentY);

                if (_currentX >= Width)
                {
                    _currentX = 0;
                    _currentY++;
                }

                return true;
            }

            public bool AddColorFragment(in int fragment)
            {
                _colorFragments.Enqueue(fragment);
                if (_colorFragments.Count > 2)
                {
                    var r = _colorFragments.Dequeue();
                    var g = _colorFragments.Dequeue();
                    var b = _colorFragments.Dequeue();
                    return AddPixel(new Color((float) r / Scale, (float) g / Scale, (float) b / Scale));
                }
                return true;
            }

            public Canvas GetCanvas() => _canvas;
        }

        private interface IPPMPart
        {
            ParseResult Process(CanvasBuilder builder);
            string GetLastFailureMessage();
        }

        private enum ParseResult
        {
            Continue,
            Failure,
            NextParser,
            Complete
        }

        private class FileTypePart : IPPMPart
        {
            private string _error;

            public FileTypePart(int typeId)
            {
                TypeId = typeId;
            }

            private int TypeId { get; }

            public ParseResult Process(CanvasBuilder builder)
            {
                try
                {
                    builder.SetFileType(TypeId);
                    return ParseResult.NextParser;
                }
                catch (InvalidOperationException ioEx)
                {
                    _error = ioEx.Message;
                    return ParseResult.Failure;
                }
            }

            public string GetLastFailureMessage() => _error;
        }

        private class ImageSizePart : IPPMPart
        {
            private string _error;

            public ImageSizePart(int x, int y)
            {
                X = x;
                Y = y;
            }

            private int X { get; }
            private int Y { get; }

            public ParseResult Process(CanvasBuilder builder)
            {
                if (X > 0 && Y > 0)
                {
                    builder.SetImageSize(X, Y);
                    return ParseResult.NextParser;
                }

                _error = "Image width and height must be greater than zero.";
                return ParseResult.Failure;
            }

            public string GetLastFailureMessage() => _error;
        }

        private class ScalePart : IPPMPart
        {
            private readonly int _scale;
            private string _error;

            public ScalePart(int scale)
            {
                _scale = scale;
            }

            public ParseResult Process(CanvasBuilder builder)
            {
                if (_scale > 0)
                {
                    builder.SetScale(_scale);
                    return ParseResult.NextParser;
                }

                _error = $"Invalid color scale of {_scale}";
                return ParseResult.Failure;
            }

            public string GetLastFailureMessage() => _error;
        }

        private class PixelDataPart : IPPMPart
        {
            private readonly int[] _colorFragments;
            private string _error;

            public PixelDataPart(IEnumerable<int> colorFragments)
            {
                _colorFragments = colorFragments.ToArray();
            }

            public ParseResult Process(CanvasBuilder builder)
            {
                try
                {
                    return _colorFragments.Any(f => !builder.AddColorFragment(f)) 
                        ? ParseResult.Complete 
                        : ParseResult.Continue;
                }
                catch (Exception ex)
                {
                    _error = ex.Message;
                    return ParseResult.Failure;
                }
            }

            public string GetLastFailureMessage() => _error;
        }
    }
}