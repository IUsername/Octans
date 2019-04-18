using System.Collections.Generic;

namespace Octans
{
    public class Scene : IScene
    {
        public IGeometry Aggregate { get; }
        public ILight[] InfiniteLights { get; }
        public ILight[] Lights { get; }

        public Scene(IGeometry aggregate, in ILight[] lights)
        {
            Aggregate = aggregate;
            Lights = lights;
            WorldBounds = aggregate.ParentSpaceBounds();
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

        public bool Intersect(in Ray r, out SurfaceInteraction si)
        {
            return Aggregate.Intersect2(in r, out si);
        }

        public bool IntersectP(in Ray r)
        {
            return Aggregate.IntersectP(in r);
        }
    }
}