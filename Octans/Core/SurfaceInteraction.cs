using System;
using Octans.Primitive;
using Octans.Reflection;

namespace Octans
{
    public class SurfaceInteraction : Interaction
    {
        public ShadingGeometry ShadingGeometry;

        // TODO: Init as readonly and update?
        public BSDF BSDF { get; } = new BSDF();

        public Point2D UV { get; set; }
        public Vector Dpdu { get; set; }
        public Vector Dpdv { get; set; }
        public Normal Dndu { get; set; }
        public Normal Dndv { get; set; }
        public IShape Shape { get; set; }

        public float Dudx { get; set; }
        public float Dvdx { get; private set; }
        public float Dudy { get; private set; }
        public float Dvdy { get; private set; }
        public Vector Dpdx { get; private set; }
        public Vector Dpdy { get; private set; }
        public IPrimitive Primitive { get; set; }

        public SurfaceInteraction Initialize(in Point p,
                                             in Vector pError,
                                             in Point2D uv,
                                             in Vector wo,
                                             in Vector dpdu,
                                             in Vector dpdv,
                                             in Normal dndu,
                                             in Normal dndv,
                                             IShape shape)
        {
            base.Initialize(in p, (Normal) Vector.Cross(in dpdu, in dpdv), in pError, in wo);

            UV = uv;
            Wo = wo;
            Dpdu = dpdu;
            Dpdv = dpdv;
            Dndu = dndu;
            Dndv = dndv;
            Shape = shape;
            ShadingGeometry.N = N;
            ShadingGeometry.Dpdu = dpdu;
            ShadingGeometry.Dpdv = dpdv;
            ShadingGeometry.Dndu = dndu;
            ShadingGeometry.Dndv = dndv;

            if (!(shape is null) && (shape.ReverseOrientation ^ shape.TransformSwapsHandedness))
            {
                N *= -1;
                ShadingGeometry.N *= -1;
            }

            BSDF.Initialize(this);


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
            Shape = other.Shape;
            ShadingGeometry = other.ShadingGeometry;
            Primitive = other.Primitive;
            BSDF.Initialize(other);
            return this;
        }

        public SurfaceInteraction Initialize(in SurfaceInteraction other, in Transform t)
        {
            // TODO: Timing, medium, face index, and bssrdf not applied.
            P = Transform.Apply(in t, other.P, other.PError, out var pError);
            PError = pError;
            N = (t * other.N).Normalize();
            Wo = (t * other.Wo).Normalize();
            UV = other.UV;
            Dpdu = t * other.Dpdu;
            Dpdv = t * other.Dpdv;
            Dndu = t * other.Dndu;
            Dndv = t * other.Dndv;
            ShadingGeometry = new ShadingGeometry
            {
                N = t * other.ShadingGeometry.N,
                Dpdu = t * other.ShadingGeometry.Dpdu,
                Dpdv = t * other.ShadingGeometry.Dpdv,
                Dndu = t * other.ShadingGeometry.Dndu,
                Dndv = t * other.ShadingGeometry.Dndv
            };
            Dudx = other.Dudx;
            Dvdx = other.Dvdx;
            Dudy = other.Dudy;
            Dvdy = other.Dvdy;
            Dpdx = other.Dpdx;
            Dpdy = other.Dpdy;
            Shape = other.Shape;
            Primitive = other.Primitive;
            ShadingGeometry.N = Normal.FaceForward(ShadingGeometry.N, N);
            BSDF.Initialize(other);
            return this;
        }

        public void ComputeScatteringFunctions(in Ray r,
                                               IObjectArena arena,
                                               bool allowMultipleLobes,
                                               TransportMode mode = TransportMode.Radiance)
        {
            ComputeDifferentials(in r);
            Primitive.ComputeScatteringFunctions(this, arena, mode, allowMultipleLobes);
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
            if (!(Shape is null) && (Shape.ReverseOrientation ^ Shape.TransformSwapsHandedness))
            {
                ShadingGeometry.N = -ShadingGeometry.N;
            }

            if (orientationIsAuthorative)
            {
                N = Normal.FaceForward(N, ShadingGeometry.N);
            }
            else
            {
                ShadingGeometry.N = Normal.FaceForward(ShadingGeometry.N, N);
            }

            ShadingGeometry.Dpdu = dpdus;
            ShadingGeometry.Dpdv = dpdvs;
            ShadingGeometry.Dndu = dndus;
            ShadingGeometry.Dndv = dndvs;
        }

        public Spectrum Le(in Vector w)
        {
            throw new NotImplementedException();
            //var areaLight = Primitive.AreaLight;
            //if (!(areaLight is null))
            //{
            //    return areaLight.L(this, w);
            //}
            //return Spectrum.Zero;
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

    public class Interaction
    {
        public Point P { get; set; }
        public Normal N { get; set; }
        public Vector PError { get; set; }
        public Vector Wo { get; set; }

        public bool IsSurfaceInteraction => !Wo.Equals(Vectors.Zero);

        public Interaction Initialize(in Point p, in Normal n, in Vector pError, in Vector wo)
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