using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octans.Pipeline
{
    public class RenderContext
    {
        public Canvas Canvas { get; }
        public RenderPipeline Pipeline { get; }

        public RenderContext(Canvas canvas, RenderPipeline pipeline)
        {
            Canvas = canvas;
            Pipeline = pipeline;
        }

        public void Render()
        {
            const int chunkSize = 4;
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