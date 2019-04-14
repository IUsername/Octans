using System.Numerics;

namespace Octans.Filter
{
    public abstract class FilterBase : IFilter
    {
        protected FilterBase(in Vector2 radius)
        {
            Radius = radius;
            InverseRadius = new Vector2(1f / radius.X, 1f / radius.Y);
        }

        public abstract float Evaluate(in Point2D p);

        public Vector2 Radius { get; }

        public Vector2 InverseRadius { get; }
    }
}