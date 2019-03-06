using System;

namespace Octans.Test
{
    public class Projectile
    {
        public Projectile(Tuple position, Tuple velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public Tuple Position { get; }
        public Tuple Velocity { get; }

        public Projectile Tick(World world)
        {
            var pos = Position + Velocity;
            var vel = Velocity + world.Gravity + world.Wind;
            return new Projectile(pos, vel);
        }

        // ReSharper disable once InconsistentNaming
        public (int x, int y) ToXY() => (x: (int) MathF.Round(Position.X), y: (int) MathF.Round(Position.Y));
    }
}