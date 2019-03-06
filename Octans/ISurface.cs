using System.Collections.Generic;

namespace Octans
{
    public interface ISurface
    {
        IReadOnlyList<Intersection> Intersect(Ray ray);
        Material Material { get; }
        Vector NormalAt(Point world);
    }
}