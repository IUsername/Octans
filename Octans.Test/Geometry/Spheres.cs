using Octans.Geometry;

namespace Octans.Test.Geometry
{
    public static class Spheres
    {
        public static Sphere GlassSphere()
        {
            var s = new Sphere {Material = {Transparency = 1.0f, RefractiveIndex = 1.5f}};
            return s;
        }
    }
}