namespace Octans.Test
{
    public class SimWorld
    {
        public SimWorld(Vector gravity, Vector wind)
        {
            Gravity = gravity;
            Wind = wind;
        }

        public Vector Gravity { get; }
        public Vector Wind { get; }
    }
}