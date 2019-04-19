using System.Collections.Generic;
using Octans.Primitive;

namespace Octans
{
    public class Scene : IScene
    {
        public IPrimitive Aggregate { get; }
        public ILight[] InfiniteLights { get; }
        public ILight[] Lights { get; }

        public Scene(IPrimitive aggregate, in ILight[] lights)
        {
            Aggregate = aggregate;
            Lights = lights;
            WorldBounds = aggregate.WorldBounds;
            var infinite = new List<ILight>();
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

        public bool Intersect(ref Ray r, ref SurfaceInteraction si)
        {
            return Aggregate.Intersect(ref r, ref si);
        }

        public bool IntersectP(ref Ray r)
        {
            return Aggregate.IntersectP(ref r);
        }
    }
}