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
}