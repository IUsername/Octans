using System.Collections.Generic;
using Octans.Primitive;
using Octans.Sampling;

namespace Octans
{
    public class Scene : IScene
    {
        public IPrimitive Aggregate { get; }
        public ILight2[] InfiniteLights { get; }
        public ILight2[] Lights { get; }

        public Scene(IPrimitive aggregate, in ILight2[] lights)
        {
            Aggregate = aggregate;
            Lights = lights;
            WorldBounds = aggregate.WorldBounds;
            var infinite = new List<ILight2>();
            foreach (var light in lights)
            {
                light.Preprocess(this);
                if (light.IsFlagged(LightType.Infinite))
                {
                    infinite.Add(light);
                }
            }

            InfiniteLights = infinite.ToArray();
        }

        public Bounds WorldBounds { get; }

        public bool Intersect(Ray r, ref SurfaceInteraction si)
        {
            return Aggregate.Intersect(ref r, ref si);
        }

        public bool IntersectP(Ray r)
        {
            return Aggregate.IntersectP(ref r);
        }

        public bool IntersectTr(Ray ray, ISampler2 sampler, ref SurfaceInteraction si, out Spectrum tr) => throw new System.NotImplementedException();
    }
}