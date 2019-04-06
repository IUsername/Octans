namespace Octans
{
    public interface IPixelRenderer
    {
        Color Render(in SubPixel sp);
    }

    public interface ICamera
    {
        Color Render(IScene scene, in SubPixel sp);
    }

    public interface IWorldShading
    {
        Color ColorFor(in Ray ray);
    }

    public interface IScene
    {
        IWorldShading World { get; }
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

    

    public class QuasiRandomSampler : ISampler
    {
        private long _index;
        private long _rInd;

        public QuasiRandomSampler(long index)
        {
            _index = index;
            _rInd = index;
        }

        public (float u, float v) NextUV()
        {
            return QuasiRandom.Next(++_index);
        }

        public float Random()
        {
            return QuasiRandom.Rand(_rInd++);
        }

        public ISampler Create(long i)
        { 
           return new QuasiRandomSampler(i);
        }
    }
}