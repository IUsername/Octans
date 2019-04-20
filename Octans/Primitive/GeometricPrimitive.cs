using Octans.Light;

namespace Octans.Primitive
{
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

    //public class TransformedPrimitive : IPrimitive
    //{
    //    public Transform PrimitiveToWorld { get; }
    //    public IPrimitive Primitive { get; }

    //    public TransformedPrimitive(Transform primitiveToWorld, IPrimitive primitive)
    //    {
    //        PrimitiveToWorld = primitiveToWorld;
    //        Primitive = primitive;
    //    }

    //    public Bounds WorldBounds => PrimitiveToWorld * Primitive.WorldBounds;
    //    public bool Intersect(ref Ray r, ref SurfaceInteraction si) => throw new System.NotImplementedException();

    //    public bool IntersectP(ref Ray r) => throw new System.NotImplementedException();

    //    public IMaterial Material => null;
    //    public AreaLight AreaLight => null;

    //    public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
    //                                           IObjectArena arena,
    //                                           TransportMode mode,
    //                                           in bool allowMultipleLobes)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}

    //public class Aggregate : IPrimitive
    //{
    //    public Aggregate()
    //    {

    //    }


    //    public Bounds WorldBounds { get; }
    //    public bool Intersect(ref Ray r, ref SurfaceInteraction si) => throw new System.NotImplementedException();

    //    public bool IntersectP(ref Ray r) => throw new System.NotImplementedException();

    //    public IMaterial Material { get; }
    //    public AreaLight AreaLight { get; }

    //    public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
    //                                           IObjectArena arena,
    //                                           TransportMode mode,
    //                                           in bool allowMultipleLobes)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}