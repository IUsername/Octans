namespace Octans
{
    public readonly struct IntersectionInfo
    {
        public IntersectionInfo(Intersection intersection, Ray ray)
        {
            T = intersection.T;
            Shape = intersection.Shape;
            Point = ray.Position(T);
            Eye = -ray.Direction;
            Normal = Shape.NormalAt(Point);
            if (Normal % Eye < 0f)
            {
                IsInside = true;
                Normal = -Normal;
            }
            else
            {
                IsInside = false;
            }

            OverPoint = Point + (Normal * Epsilon);
        }

        public readonly float T;
        public readonly IShape Shape;
        public readonly Point Point;
        public readonly Point OverPoint;
        public readonly Vector Eye;
        public readonly Vector Normal;
        public readonly bool IsInside;

        public const float Epsilon = 0.002f;
    }
}