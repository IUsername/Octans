namespace Octans.Primitive
{
    public abstract class Shape : IShape
    {
        protected Shape(Transform objectToWorld,
                        Transform worldToObject,
                        bool reverseOrientation)
        {
            ObjectToWorld = objectToWorld;
            WorldToObject = worldToObject;
            ReverseOrientation = reverseOrientation;
            TransformSwapsHandedness = ObjectToWorld.SwapsHandedness();
        }

        public Transform ObjectToWorld { get; }
        public Transform WorldToObject { get; }
        public bool ReverseOrientation { get; }

        public bool TransformSwapsHandedness { get; }
        public abstract float Area();

        public abstract bool Intersect(in Ray r,
                                       out float tHit,
                                       ref SurfaceInteraction si,
                                       bool testAlphaTexture = true);

        public abstract bool IntersectP(in Ray r, bool testAlphaTexture = true);

        //public virtual float SolidAngle(in Point p, int nSamples = 512)
        //{
        //    var r = new Interaction();
        //    r.Initialize(p, new Normal(), new Vector(), Vectors.ZAxis);
        //    double solidAngle = 0;
        //    for (var i = 0; i < nSamples; ++i)
        //    {
        //        var u = new Point2D(QuasiRandom.RadicalInverse(0, i),
        //                            QuasiRandom.RadicalInverse(1, i));
        //        var pShape = Sample(ref r, u, out var pdf);
        //        var ray = new Ray(p, pShape.P - p);
        //        if (pdf > 0f && !IntersectP(ref ray))
        //        {
        //            solidAngle += 1f / pdf;
        //        }
        //    }

        //    return (float) (solidAngle / nSamples);
        //}

        public virtual Interaction Sample(Interaction refP, in Point2D u, out float pdf)
        {
            var interaction = Sample(u, out pdf);
            var wi = interaction.P - refP.P;
            if (wi.MagSqr() == 0f)
            {
                pdf = 0f;
            }
            else
            {
                wi = wi.Normalize();
                pdf *= Point.DistanceSqr(refP.P, interaction.P) / System.MathF.Abs(interaction.N % -wi);
                if (float.IsInfinity(pdf))
                {
                    pdf = 0f;
                }
            }

            return interaction;
        }

        public abstract Interaction Sample(in Point2D u, out float pdf);

        public virtual float Pdf(Interaction r, in Vector wi)
        {
            var ray = r.SpawnRay(wi);
            var isectLight = new SurfaceInteraction();
            if (!Intersect(ray, out _, ref isectLight, false))
            {
                return 0f;
            }

            var pdf = Point.DistanceSqr(r.P, isectLight.P) / (System.MathF.Abs(isectLight.N % -wi) * Area());
            if (float.IsInfinity(pdf))
            {
                pdf = 0f;
            }

            return pdf;
        }

        public virtual float Pdf(Interaction r) => 1f / Area();

        public abstract Bounds ObjectBounds { get; }

        public virtual Bounds WorldBounds => ObjectToWorld * ObjectBounds;
    }
}