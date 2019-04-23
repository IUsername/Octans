using Octans.Primitive;
using Octans.Reflection;
using static System.MathF;
using static System.Single;
using static Octans.Utilities;

namespace Octans
{
    public sealed class SurfaceInteraction : Interaction
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
            base.Initialize(in p, ((Normal) Vector.Cross(in dpdu, in dpdv)).Normalize(), in pError, in wo);

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

            if (!(shape is null) && shape.ReverseOrientation ^ shape.TransformSwapsHandedness)
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
            BSDF.Initialize(this);
            return this;
        }

        public void ComputeScatteringFunctions(in RayDifferential r,
                                               IObjectArena arena,
                                               bool allowMultipleLobes = true,
                                               TransportMode mode = TransportMode.Radiance)
        {
            ComputeDifferentials(in r);
            Primitive.ComputeScatteringFunctions(this, arena, mode, allowMultipleLobes);
        }

        private void ComputeDifferentials(in RayDifferential ray)
        {
            if (ray.HasDifferentials)
            {
                var d = N % (Vector) P;
                var tx = -(N % (Vector) ray.RxOrigin - d) / (N % ray.RxDirection);
                if (IsInfinity(tx) || IsNaN(tx))
                {
                    DifferentialFailure();
                    return;
                }

                var px = ray.RxOrigin + tx * ray.RxDirection;

                var ty = -(N % (Vector) ray.RyOrigin - d) / (N % ray.RyDirection);
                if (IsInfinity(ty) || IsNaN(ty))
                {
                    DifferentialFailure();
                    return;
                }

                var py = ray.RyOrigin + ty * ray.RyDirection;

                Dpdx = px - P;
                Dpdy = py - P;

                var dim = new int[2];
                if (Abs(N.X) > Abs(N.Y) && Abs(N.X) > Abs(N.Z))
                {
                    dim[0] = 1;
                    dim[1] = 2;
                }
                else if (Abs(N.Y) > Abs(N.Z))
                {
                    dim[0] = 0;
                    dim[1] = 2;
                }
                else
                {
                    dim[0] = 0;
                    dim[1] = 1;
                }

                float[][] A =
                {
                    new[] {Dpdu[dim[0]], Dpdv[dim[0]]},
                    new[] {Dpdu[dim[1]], Dpdv[dim[1]]}
                };

                float[] Bx = {px[dim[0]] - P[dim[0]], px[dim[1]] - P[dim[1]]};
                float[] By = {py[dim[0]] - P[dim[0]], py[dim[1]] - P[dim[1]]};

                if (SolveLinearSystem2x2(A, Bx, out var dudx, out var dvdx))
                {
                    Dudx = dudx;
                    Dvdx = dvdx;
                }
                else
                {
                    Dudx = 0f;
                    Dvdx = 0f;
                }

                if (SolveLinearSystem2x2(A, By, out var dudy, out var dvdy))
                {
                    Dudy = dudy;
                    Dvdy = dvdy;
                }
                else
                {
                    Dudy = 0f;
                    Dvdy = 0f;
                }
            }
            else
            {
                DifferentialFailure();
            }
        }

        private void DifferentialFailure()
        {
            Dudx = Dvdx = 0f;
            Dudy = Dvdy = 0f;
            Dpdx = Vectors.Zero;
            Dpdy = Vectors.Zero;
        }

        public void SetShadingGeometry(in Vector dpdus,
                                       in Vector dpdvs,
                                       in Normal dndus,
                                       in Normal dndvs,
                                       bool orientationIsAuthoritative)
        {
            ShadingGeometry.N = ((Normal) Vector.Cross(dpdus, dpdvs)).Normalize();
            if (!(Shape is null) && Shape.ReverseOrientation ^ Shape.TransformSwapsHandedness)
            {
                ShadingGeometry.N = -ShadingGeometry.N;
            }

            if (orientationIsAuthoritative)
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
            var area = Primitive.AreaLight;
            return area is null ? Spectrum.Zero : area.L(this, w);
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
        public Interaction()
        {
        }

        public Interaction(in Point p)
        {
            P = p;
        }

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

        public Ray SpawnRayTo(Interaction it)
        {
            var o = OffsetRayOrigin(P, PError, N, it.P - P);
            var t = OffsetRayOrigin(it.P, it.PError, it.N, o - it.P);
            var d = t - o;
            return new Ray(o, d, 1 - MathF.ShadowEpsilon);
        }
    }
}