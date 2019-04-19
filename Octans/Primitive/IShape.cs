using Octans.Light;

namespace Octans.Primitive
{
    public interface IShape
    {
        Transform ObjectToWorld { get; }
        Transform WorldToObject { get; }
        bool ReverseOrientation { get; }
        bool TransformSwapsHandedness { get; }

        float Area();

        bool Intersect(in Ray r, out float tHit, ref SurfaceInteraction si, bool testAlphaTexture = true);
        bool IntersectP(in Ray r, bool testAlphaTexture = true);
        //float SolidAngle(in Point p, int nSamples = 512);

        //Interaction Sample(ref Interaction r, Point2D u, out float pdf);
        //Interaction Sample(Point2D u, out float pdf);
        //float Pdf(ref Interaction r, in Vector wi);
        //float Pdf(ref Interaction r);

        Bounds ObjectBounds { get; }

        Bounds WorldBounds { get; }
    }

    public interface IPrimitive
    {
        Bounds WorldBounds { get; }

        bool Intersect(ref Ray r, ref SurfaceInteraction si);

        bool IntersectP(ref Ray r);

         IMaterial Material { get; }

         AreaLight AreaLight { get; }

        void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                        IObjectArena arena,
                                        TransportMode mode,
                                        in bool allowMultipleLobes);
    }

    public class GeometricPrimitive : IPrimitive
    {
        public IShape Shape { get; }
        public IMaterial Material { get; }
        public AreaLight AreaLight { get; }

        public GeometricPrimitive(IShape shape, IMaterial material, AreaLight areaLight)
        {
            Shape = shape;
            Material = material;
            AreaLight = areaLight;
        }

        public Bounds WorldBounds => Shape.WorldBounds;

        public bool Intersect(ref Ray r, ref SurfaceInteraction si)
        {
            if (!Shape.Intersect(in r, out var tHit, ref si)) return false;
            r.TMax = tHit;
            si.Primitive = this;
            // TODO: Handle medium.
            return true;
        }

        public bool IntersectP(ref Ray r) => Shape.IntersectP(r);
      

        public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               in bool allowMultipleLobes)
        {
            Material?.ComputeScatteringFunctions(surfaceInteraction, arena, mode, allowMultipleLobes);
        }
    }
}