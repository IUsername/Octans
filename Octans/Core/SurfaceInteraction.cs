using Octans.Reflection;

namespace Octans
{
    public class SurfaceInteraction : Interaction
    {
        public ShadingGeometry ShadingGeometry;

        // TODO: Init as readonly and update?
        public BSDF BSDF { get; } = new BSDF();

        public Point2D UV { get; set; }
        public Vector Dpdu { get; private set; }
        public Vector Dpdv { get; private set; }
        public Normal Dndu { get; private set; }
        public Normal Dndv { get; private set; }
        public IGeometry Geometry { get; private set; }

        public float Dudx { get; private set; }
        public float Dvdx { get; private set; }
        public float Dudy { get; private set; }
        public float Dvdy { get; private set; }
        public Vector Dpdx { get; private set; }
        public Vector Dpdy { get; private set; }

        public SurfaceInteraction Initialize(in Point p,
                                             in Vector pError,
                                             in Point2D uv,
                                             in Vector wo,
                                             in Vector dpdu,
                                             in Vector dpdv,
                                             in Normal dndu,
                                             in Normal dndv,
                                             IGeometry geometry)
        {
            base.Initialize(in p, (Normal) Vector.Cross(in dpdu, in dpdv), in pError, in wo);

            UV = uv;
            Wo = wo;
            Dpdu = dpdu;
            Dpdv = dpdv;
            Dndu = dndu;
            Dndv = dndv;
            Geometry = geometry;
            ShadingGeometry.N = N;
            ShadingGeometry.Dpdu = dpdu;
            ShadingGeometry.Dpdv = dpdv;
            ShadingGeometry.Dndu = dndu;
            ShadingGeometry.Dndv = dndv;

            // TODO: Adjust orientation


            return this;
        }

        // TODO: Temporary
        public SurfaceInteraction InitializeT(in Point p,
                                             in Vector pError,
                                             in Point2D uv,
                                             in Vector wo,
                                             in Normal n,
                                             in Vector dpdu,
                                             in Vector dpdv,
                                             in Normal dndu,
                                             in Normal dndv,
                                             IGeometry geometry)
        {
            base.Initialize(in p, in n, in pError, in wo);

            UV = uv;
            Wo = wo;
            Dpdu = dpdu;
            Dpdv = dpdv;
            Dndu = dndu;
            Dndv = dndv;
            Geometry = geometry;
            ShadingGeometry.N = N;
            ShadingGeometry.Dpdu = dpdu;
            ShadingGeometry.Dpdv = dpdv;
            ShadingGeometry.Dndu = dndu;
            ShadingGeometry.Dndv = dndv;

            // TODO: Adjust orientation


            return this;
        }

        public SurfaceInteraction Initialize(in SurfaceInteraction other)
        {
            P = other.P;
            PError = other.PError;
            N = other.N;
            Wo = other.Wo;
            UV = other.UV;
            Dpdu = other.Dpdu;
            Dpdv = other.Dpdv;
            Dndu = other.Dndu;
            Dndv = other.Dndv;
            Geometry = other.Geometry;
            ShadingGeometry = other.ShadingGeometry;
            BSDF.Initialize(this);
            return this;
        }

        public void ComputeScatteringFunctions(in Ray r,
                                               IObjectArena arena,
                                               bool allowMultipleLobes,
                                               TransportMode mode = TransportMode.Radiance)
        {
            ComputeDifferentials(in r);
            Geometry.ComputeScatteringFunctions(this, arena, mode, allowMultipleLobes);
        }

        private void ComputeDifferentials(in Ray ray)
        {
            // TODO
            Dudx = Dvdx = 0f;
            Dudy = Dvdy = 0f;
            Dpdx = Vectors.Zero;
            Dpdy = Vectors.Zero;
        }

        public void SetShadingGeometry(in Vector dpdus,
                                       in Vector dpdvs,
                                       in Normal dndus,
                                       in Normal dndvs,
                                       bool orientationIsAuthorative)
        {
            ShadingGeometry.N = ((Normal) Vector.Cross(dpdus, dpdvs)).Normalize();
            // TODO: Orientation

            if (orientationIsAuthorative)
            {
                N = Normal.FaceForward(N, ShadingGeometry.N);
            }
            else
            {
                ShadingGeometry.N = Normal.FaceForward(ShadingGeometry.N, N);
            }
        }

        public Spectrum Le(in Vector vector)
        {
            throw new System.NotImplementedException();
        }
    }

    public struct ShadingGeometry
    {
        public Normal N;
        public Vector Dpdu;
        public Vector Dpdv;
        public Normal Dndu;
        public Normal Dndv;
    }

    public abstract class Interaction
    {
        public Point P { get; set; }
        public Normal N { get; set; }
        public Vector PError { get; set; }
        public Vector Wo { get; set; }

        public bool IsSurfaceInteraction => !Wo.Equals(Vectors.Zero);

        protected Interaction Initialize(in Point p, in Normal n, in Vector pError, in Vector wo)
        {
            P = p;
            N = n;
            PError = pError;
            Wo = wo.Normalize();
            return this;
        }

        public Ray SpawnRay(in Vector d)
        {
            var o = OffsetRayOrigin(P, PError, N, in d);
            return new Ray(o, d);
        }

        private static Point OffsetRayOrigin(in Point p, in Vector pError, in Normal n, in Vector w)
        {
            // TODO: Account for pError
            var delta = n * 0.00001f;
            return p + (Vector) delta;
        }
    }
}