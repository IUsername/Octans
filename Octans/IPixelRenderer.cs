namespace Octans
{
    public interface IPixelRenderer
    {
        Color Render(in SubPixel sp);
    }


    public interface IPixelSampler
    {
        Color Gather(in PixelInformation pixel, ISampler sampler, IPixelRenderSegment segment);
    }

    public interface ICameraSampler
    {
        (Ray ray, float throughput) CameraRay(in PixelSample sample, ISampler sampler);
    }

    public interface IWorldSampler
    {
        Color Sample(in Ray ray, ISampler sampler);
    }

    public interface IPixelRenderSegment
    {
        Color Render(in PixelSample sample, ISampler sampler);
    }

    public interface ISampler
    {
        (float u, float v) NextUV();

        float Random();

        ISampler Create(long i);
    }

    public class RenderPipeline : IPixelRenderSegment
    {
        public IWorldSampler World { get; }
        public ICameraSampler Camera { get; }
        public IPixelSampler Film { get; }

        public RenderPipeline(IWorldSampler world, ICameraSampler camera, IPixelSampler film)
        {
            World = world;
            Camera = camera;
            Film = film;
        }

        public Color Render(in PixelSample sample, ISampler sampler)
        {
            var (ray, throughput) = Camera.CameraRay(in sample, sampler);
            return World.Sample(in ray, sampler) * throughput;
        }

        public Color Capture(in PixelInformation pixel, ISampler sampler)
        {
           return Film.Gather(in pixel, sampler, this);
        }
    }
}