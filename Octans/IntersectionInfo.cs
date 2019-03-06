namespace Octans
{
    public readonly struct IntersectionInfo
    {
        public IntersectionInfo(Intersection intersection, Ray ray)
        {
            T = intersection.T;
            Surface = intersection.Surface;
            Point = ray.Position(T);
            Eye = -ray.Direction;
            Normal = Surface.NormalAt(Point);
            if (Normal % Eye < 0f)
            {
                IsInside = true;
                Normal = -Normal;
            }
            else
            {
                IsInside = false;
            }
        }

        public readonly float T;
        public readonly ISurface Surface;
        public readonly Point Point;
        public readonly Vector Eye;
        public readonly Vector Normal;
        public readonly bool IsInside;
    }
}