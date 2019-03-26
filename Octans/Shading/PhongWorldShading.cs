namespace Octans.Shading
{
    public class PhongWorldShading : IWorldShading
    {
        private readonly World _world;

        public int MaxBounces { get; }

        public PhongWorldShading(int maxBounces, World world)
        {
            _world = world;
            MaxBounces = maxBounces;
        }

        public Color ColorFor(in Ray ray)
        {
            return PhongShading.ColorAt(_world, in ray, MaxBounces);
        }
    }
}