using Octans.Sampling;

namespace Octans
{
    public class VisibilityTester
    {
        public Interaction P0 { get; }
        public Interaction P1 { get; }

        public VisibilityTester(Interaction p0, Interaction p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public bool Unoccluded(IScene scene)
        {
            return !scene.IntersectP(P0.SpawnRayTo(P1));
        }

        public Spectrum Tr(IScene scene, ISampler sampler)
        {
            var ray = P0.SpawnRayTo(P1);
            Spectrum tr = Spectrum.One;
            while (true)
            {
                var si = new SurfaceInteraction();
                var hitSurface = scene.Intersect(ray, ref si);
                if (hitSurface && !(si.Primitive.Material is null))
                {
                    return Spectrum.Zero;
                }

                if (!(ray.Medium is null))
                {
                    tr *= ray.Medium.Tr(ray, sampler);
                }

                if (!hitSurface)
                {
                    break;
                }
                ray = si.SpawnRayTo(P1);
            }

            return tr;

        }
    }
}