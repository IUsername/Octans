using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octans.Pipeline
{
    public static class RenderContext
    {
        public static void Render(Canvas canvas, IPixelRenderer renderer)
        {
            var queue = new ConcurrentQueue<(int x, int y)>();
            for (var y = 0; y < canvas.Height; y++)
            {
                for (var x = 0; x < canvas.Width; x++)
                {
                    queue.Enqueue((x, y));
                }
            }
            Parallel.ForEach(queue, p => RenderToCanvas(p.x, p.y, canvas, renderer));
        }

        private static void RenderToCanvas(int x, int y, Canvas canvas, IPixelRenderer renderer)
        {
            var sp = SubPixel.ForPixelCenter(x, y);
            var c = renderer.Render(in sp);
            canvas.WritePixel(in c, x, y);
        }
    }

    public class RenderContext2
    {
        public Canvas Canvas { get; }
        public RenderPipeline Pipeline { get; }

        public RenderContext2(Canvas canvas, RenderPipeline pipeline)
        {
            Canvas = canvas;
            Pipeline = pipeline;
        }

        public void Render()
        {
            const int chunkSize = 16;
            var queue = new ConcurrentQueue<FilmArea>();
            for (var y = 0; y < Canvas.Height;)
            {
                var yEnd = Math.Min(Canvas.Height, y + chunkSize);
                for (var x = 0; x < Canvas.Width;)
                {
                    var xEnd = Math.Min(Canvas.Width, x + chunkSize);
                    var min = new PixelCoordinate(x, y);
                    var max = new PixelCoordinate(xEnd, yEnd);
                    queue.Enqueue(new FilmArea(min, max));
                    x = xEnd;
                }

                y = yEnd;
            }

            var sampler = new QuasiRandomSampler(0);
            Parallel.ForEach(queue, a => RenderToCanvas(a, Canvas, Pipeline, sampler));
        }

        private static void RenderToCanvas(FilmArea area, Canvas canvas, RenderPipeline pipeline, ISampler sampler)
        {
            for (var y = area.Min.Y; y < area.Max.Y; y++)
            {
                for (var x = area.Min.X; x < area.Max.X; x++)
                {
                    var pixel = new PixelCoordinate(x, y);
                    var c = pipeline.Capture(new PixelInformation(pixel, canvas.Width, canvas.Height), sampler);
                    canvas.WritePixel(in c, pixel.X, pixel.Y);
                }
            }
        }
    }
}