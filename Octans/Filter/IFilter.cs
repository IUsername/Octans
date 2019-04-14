using System.Numerics;

namespace Octans.Filter
{
    public interface IFilter
    {
        float Evaluate(in Point2D p);
        Vector2 Radius { get; }
    }
}