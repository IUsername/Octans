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
}