namespace Octans.Test
{
    public class World
    {
        public World(Vector gravity, Vector wind)
        {
            Gravity = gravity;
            Wind = wind;
        }

        public Vector Gravity { get; }
        public Vector Wind { get; }
    }
}