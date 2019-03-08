namespace Octans
{
    public readonly struct Ray
    {
        public readonly Point Origin;
        public readonly Vector Direction;

        public Ray(Point origin, Vector direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Point Position(float t) => Origin + Direction * t;

        public Ray Transform(in Matrix matrix)
        {
            var origin = matrix * Origin;
            var direction = matrix * Direction;
            return new Ray(origin, direction);
        }
    }
}