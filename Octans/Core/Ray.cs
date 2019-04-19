using static System.Single;

namespace Octans
{
    public struct Ray
    {
        public float TMax { get; set; }
        public readonly Point Origin;
        public readonly Vector Direction;
        public readonly Vector InverseDirection;

        public Ray(Point origin, Vector direction, float tMax = PositiveInfinity)
        {
            TMax = tMax;
            Origin = origin;
            Direction = direction;
            InverseDirection = 1f / direction;
        }

        public Point Position(float t) => Origin + Direction * t;
    }
}