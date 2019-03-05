namespace Octans.Test
{
    public class World
    {
        public World(Tuple gravity, Tuple wind)
        {
            Gravity = gravity;
            Wind = wind;
        }

        public Tuple Gravity { get; }
        public Tuple Wind { get; }
    }
}