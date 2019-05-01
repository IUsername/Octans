using System;
using static System.MathF;
using static Octans.MathF;
using static Octans.Point;
using static Octans.Vector;

namespace Octans.Primitive
{
    public sealed class Triangle : Shape
    {
        private readonly TriangleMesh _mesh;
        private readonly ReadOnlyMemory<int> _v;
        private readonly int _faceIndex;

        public Triangle(Transform objectToWorld,
                        Transform worldToObject,
                        bool reverseOrientation,
                        TriangleMesh mesh,
                        int triNumber)
            : base(objectToWorld, worldToObject, reverseOrientation)
        {
            _mesh = mesh;
            _v = mesh.VertexIndicesSpan(triNumber);
            _faceIndex = mesh.HasFaceIndices ? mesh.FaceIndices[triNumber] : 0;
        }

        public override Bounds ObjectBounds
        {
            get
            {
                var s = _v.Span;
                var p0 = WorldToObject * _mesh.P[s[0]];
                var p1 = WorldToObject * _mesh.P[s[1]];
                var p2 = WorldToObject * _mesh.P[s[2]];
                return Bounds.FromPoints(p0, p1, p2);
            }
        }

        public override Bounds WorldBounds
        {
            get
            {
                var s = _v.Span;
                var p0 = _mesh.P[s[0]];
                var p1 = _mesh.P[s[1]];
                var p2 = _mesh.P[s[2]];
                return Bounds.FromPoints(p0, p1, p2);
            }
        }

        public override float Area()
        {
            var s = _v.Span;
            var p0 = _mesh.P[s[0]];
            var p1 = _mesh.P[s[1]];
            var p2 = _mesh.P[s[2]];
            return 0.5f * Cross(p1 - p0, p2 - p0).Length();
        }

        public override bool Intersect(in Ray r,
                                       out float tHit,
                                       ref SurfaceInteraction si,
                                       bool testAlphaTexture = true)
        {
            // Get triangle vertices
            var s = _v.Span;
            var p0 = _mesh.P[s[0]];
            var p1 = _mesh.P[s[1]];
            var p2 = _mesh.P[s[2]];

            // Perform ray--triangle intersection test

            // Transform triangle vertices to ray coordinate space

            // Translate vertices based on ray origin
            var p0t = p0 - (Vector) r.Origin;
            var p1t = p1 - (Vector) r.Origin;
            var p2t = p2 - (Vector) r.Origin;

            // Permute components of triangle vertices and ray direction
            var kz = MaxDimension(Abs(r.Direction));
            var kx = kz + 1;
            if (kx == 3)
            {
                kx = 0;
            }

            var ky = kx + 1;
            if (ky == 3)
            {
                ky = 0;
            }

            var d = Permute(r.Direction, kx, ky, kz);
            p0t = Permute(p0t, kx, ky, kz);
            p1t = Permute(p1t, kx, ky, kz);
            p2t = Permute(p2t, kx, ky, kz);

            // Apply shear transformation to translated vertex positions
            var Sx = -d.X / d.Z;
            var Sy = -d.Y / d.Z;
            var Sz = 1f / d.Z;
            p0t.X += Sx * p0t.Z;
            p0t.Y += Sy * p0t.Z;
            p1t.X += Sx * p1t.Z;
            p1t.Y += Sy * p1t.Z;
            p2t.X += Sx * p2t.Z;
            p2t.Y += Sy * p2t.Z;

            // Compute edge function coefficients
            var e0 = p1t.X * p2t.Y - p1t.Y * p2t.X;
            var e1 = p2t.X * p0t.Y - p2t.Y * p0t.X;
            var e2 = p0t.X * p1t.Y - p0t.Y * p1t.X;

            // Fall back to double precision test at triangle edges
            if (e0 == 0.0f || e1 == 0.0f || e2 == 0.0f)
            {
                var p2txp1ty = p2t.X * (double) p1t.Y;
                var p2typ1tx = p2t.Y * (double) p1t.X;
                e0 = (float) (p2typ1tx - p2txp1ty);
                var p0txp2ty = p0t.X * (double) p2t.Y;
                var p0typ2tx = p0t.Y * (double) p2t.X;
                e1 = (float) (p0typ2tx - p0txp2ty);
                var p1txp0ty = p1t.X * (double) p0t.Y;
                var p1typ0tx = p1t.Y * (double) p0t.X;
                e2 = (float) (p1typ0tx - p1txp0ty);
            }

            // Perform triangle edge and determinant tests
            if ((e0 < 0 || e1 < 0 || e2 < 0) && (e0 > 0 || e1 > 0 || e2 > 0))
            {
                tHit = 0f;
                return false;
            }

            var det = e0 + e1 + e2;
            if (det == 0)
            {
                tHit = 0f;
                return false;
            }

            // Compute scaled hit distance to triangle and test against ray 't' range
            p0t.Z *= Sz;
            p1t.Z *= Sz;
            p2t.Z *= Sz;
            var tScaled = e0 * p0t.Z + e1 * p1t.Z + e2 * p2t.Z;
            if (det < 0 && (tScaled >= 0 || tScaled < r.TMax * det))
            {
                tHit = 0f;
                return false;
            }

            if (det > 0 && (tScaled <= 0 || tScaled > r.TMax * det))
            {
                tHit = 0f;
                return false;
            }

            // Compute barycentric coordinates and 't' value for triangle intersection
            var invDet = 1 / det;
            var b0 = e0 * invDet;
            var b1 = e1 * invDet;
            var b2 = e2 * invDet;
            var t = tScaled * invDet;

            // Ensure that computed triangle 't' is conservatively greater than zero

            // Compute 'delta_z' term for triangle 't' error bounds
            var maxZt = MaxDimension(Abs(new Vector(p0t.Z, p1t.Z, p2t.Z)));
            var deltaZ = Gamma(3) * maxZt;

            // Compute 'delta_x' and 'delta_y' terms for triangle 't' error bounds
            var maxXt = MaxDimension(Abs(new Vector(p0t.X, p1t.X, p2t.X)));
            var maxYt = MaxDimension(Abs(new Vector(p0t.Y, p1t.Y, p2t.Y)));
            var deltaX = Gamma(5) * (maxXt + maxZt);
            var deltaY = Gamma(5) * (maxYt + maxZt);

            // Compute 'delta_e' term for triangle 't' error bounds
            var deltaE = 2 * (Gamma(2) * maxXt * maxYt + deltaY * maxXt + deltaX * maxYt);

            // Compute 'delta_t' term for triangle 't' error bounds and check _t_
            var maxE = MaxDimension(Abs(new Vector(e0, e1, e2)));
            var deltaT = 3 * (Gamma(3) * maxE * maxZt + deltaE * maxZt + deltaZ * maxE) *
                         Abs(invDet);
            if (t <= deltaT)
            {
                tHit = 0f;
                return false;
            }

            // Compute triangle partial derivatives
            var dpdu = Vectors.Zero;
            var dpdv = Vectors.Zero;
            GetUVs(out var uv);

            // Compute deltas for triangle partial derivatives
            var duv02 = uv[0] - uv[2];
            var duv12 = uv[1] - uv[2];
            var dp02 = p0 - p2;
            var dp12 = p1 - p2;
            var determinant = duv02.X * duv12.Y - duv02.Y * duv12.X;
            var degenerateUV = Abs(determinant) < 1e-8;
            if (!degenerateUV)
            {
                invDet = 1f / determinant;
                dpdu = (duv12.Y * dp02 - duv02.Y * dp12) * invDet;
                dpdv = (-duv12.X * dp02 + duv02.X * dp12) * invDet;
            }

            if (degenerateUV || Cross(dpdu, dpdv).LengthSquared() == 0)
            {
                // Handle zero determinant for triangle partial derivative matrix
                var ng = Cross(p2 - p0, p1 - p0);
                if (ng.LengthSquared() == 0)
                    // The triangle is actually degenerate; the intersection is
                    // bogus.
                {
                    tHit = 0f;
                    return false;
                }

                (dpdu, dpdv) = OrthonormalPosZ((Normal) Cross(p2 - p0, p1 - p0).Normalize());
            }

            // Compute error bounds for triangle intersection
            var xAbsSum = Abs(b0 * p0.X) + Abs(b1 * p1.X) + Abs(b2 * p2.X);
            var yAbsSum = Abs(b0 * p0.Y) + Abs(b1 * p1.Y) + Abs(b2 * p2.Y);
            var zAbsSum = Abs(b0 * p0.Z) + Abs(b1 * p1.Z) + Abs(b2 * p2.Z);
            var pError = Gamma(7) * new Vector(xAbsSum, yAbsSum, zAbsSum);

            // Interpolate $(u,v)$ parametric coordinates and hit point
            var pHit = Sum(b0 * p0, b1 * p1, b2 * p2);
            var uvHit = Point2D.Sum(b0 * uv[0], b1 * uv[1], b2 * uv[2]);

            // Test intersection against alpha texture, if present
            if (testAlphaTexture && _mesh.HasAlphaMask)
            {
                var isectLocal = new SurfaceInteraction();
                isectLocal.Initialize(pHit, Vectors.Zero, uvHit, -r.Direction, dpdu, dpdv,
                                      Normals.Zero, Normals.Zero, this);
                if (_mesh.AlphaMask.Evaluate(isectLocal) == 0)
                {
                    tHit = 0f;
                    return false;
                }
            }

            // Fill in 'SurfaceInteraction' from triangle hit
            si.Initialize(pHit, pError, uvHit, -r.Direction, dpdu, dpdv, Normals.Zero, Normals.Zero, this, _faceIndex);

            //*isect = SurfaceInteraction(pHit, pError, uvHit, -ray.d, dpdu, dpdv,
            //                            Normal3f(0, 0, 0), Normal3f(0, 0, 0), ray.time,
            //                            this, faceIndex);

            // Override surface normal in _isect_ for triangle
            si.N = si.ShadingGeometry.N = (Normal) Cross(dp02, dp12).Normalize();
            if (_mesh.HasN || _mesh.HasS)
            {
                // Initialize _Triangle_ shading geometry

                // Compute shading normal _ns_ for triangle
                var v = _v.Span;
                Normal ns;
                if (_mesh.HasN)
                {
                    ns = b0 * _mesh.N[v[0]] + b1 * _mesh.N[v[1]] + b2 * _mesh.N[v[2]];
                    ns = ns.MagSqr() > 0 ? ns.Normalize() : si.N;
                }
                else
                {
                    ns = si.N;
                }

                // Compute shading tangent _ss_ for triangle
                Vector ss;
                if (_mesh.HasS)
                {
                    ss = b0 * _mesh.S[v[0]] + b1 * _mesh.S[v[1]] + b2 * _mesh.S[v[2]];
                    ss = ss.LengthSquared() > 0 ? ss.Normalize() : si.Dpdu.Normalize();
                }
                else
                {
                    ss = si.Dpdu.Normalize();
                }

                // Compute shading bitangent _ts_ for triangle and adjust _ss_
                var ts = Cross(ss, ns);
                if (ts.LengthSquared() > 0f)
                {
                    ts = ts.Normalize();
                    ss = Cross(ts, ns);
                }
                else
                {
                    (ss, ts) = OrthonormalPosZ(ns);
                }

                // Compute 'dndu' and 'dndv' for triangle shading geometry
                Normal dndu;
                Normal dndv;
                if (_mesh.HasN)
                {
                    // Compute deltas for triangle partial derivatives of normal
                    duv02 = uv[0] - uv[2];
                    duv12 = uv[1] - uv[2];
                    var dn1 = _mesh.N[v[0]] - _mesh.N[v[2]];
                    var dn2 = _mesh.N[v[1]] - _mesh.N[v[2]];
                    determinant = duv02.X * duv12.Y - duv02.Y * duv12.X;
                    degenerateUV = Abs(determinant) < 1e-8;
                    if (degenerateUV)
                    {
                        // We can still compute dndu and dndv, with respect to the
                        // same arbitrary coordinate system we use to compute dpdu
                        // and dpdv when this happens. It's important to do this
                        // (rather than giving up) so that ray differentials for
                        // rays reflected from triangles with degenerate
                        // parameterizations are still reasonable.
                        var dn = Cross((Vector) (_mesh.N[v[2]] - _mesh.N[v[0]]),
                                       _mesh.N[v[1]] - _mesh.N[v[0]]);

                        if (dn.LengthSquared() == 0)
                        {
                            dndu = dndv = Normals.Zero;
                        }
                        else
                        {
                            var (dnu, dnv) = OrthonormalPosZ((Normal) dn);
                            dndu = (Normal) dnu;
                            dndv = (Normal) dnv;
                        }
                    }
                    else
                    {
                        invDet = 1f / determinant;
                        dndu = (duv12.Y * dn1 - duv02.Y * dn2) * invDet;
                        dndv = (-duv12.X * dn1 + duv02.X * dn2) * invDet;
                    }
                }
                else
                {
                    dndu = dndv = Normals.Zero;
                }

                si.SetShadingGeometry(ss, ts, dndu, dndv, true);
            }

            // Ensure correct orientation of the geometric normal
            if (_mesh.HasN)
            {
                si.N = Normal.FaceForward(si.N, si.ShadingGeometry.N);
            }
            else if (ReverseOrientation ^ TransformSwapsHandedness)
            {
                si.N = si.ShadingGeometry.N = -si.N;
            }

            tHit = t;

            return true;
        }

        public override bool IntersectP(in Ray r, bool testAlphaTexture = true)
        {
            // Get triangle vertices
            var s = _v.Span;
            var p0 = _mesh.P[s[0]];
            var p1 = _mesh.P[s[1]];
            var p2 = _mesh.P[s[2]];

            // Perform ray--triangle intersection test

            // Transform triangle vertices to ray coordinate space

            // Translate vertices based on ray origin
            var p0t = p0 - (Vector) r.Origin;
            var p1t = p1 - (Vector) r.Origin;
            var p2t = p2 - (Vector) r.Origin;

            // Permute components of triangle vertices and ray direction
            var kz = MaxDimension(Abs(r.Direction));
            var kx = kz + 1;
            if (kx == 3)
            {
                kx = 0;
            }

            var ky = kx + 1;
            if (ky == 3)
            {
                ky = 0;
            }

            var d = Permute(r.Direction, kx, ky, kz);
            p0t = Permute(p0t, kx, ky, kz);
            p1t = Permute(p1t, kx, ky, kz);
            p2t = Permute(p2t, kx, ky, kz);

            // Apply shear transformation to translated vertex positions
            var Sx = -d.X / d.Z;
            var Sy = -d.Y / d.Z;
            var Sz = 1f / d.Z;
            p0t.X += Sx * p0t.Z;
            p0t.Y += Sy * p0t.Z;
            p1t.X += Sx * p1t.Z;
            p1t.Y += Sy * p1t.Z;
            p2t.X += Sx * p2t.Z;
            p2t.Y += Sy * p2t.Z;

            // Compute edge function coefficients
            var e0 = p1t.X * p2t.Y - p1t.Y * p2t.X;
            var e1 = p2t.X * p0t.Y - p2t.Y * p0t.X;
            var e2 = p0t.X * p1t.Y - p0t.Y * p1t.X;

            // Fall back to double precision test at triangle edges
            if (e0 == 0.0f || e1 == 0.0f || e2 == 0.0f)
            {
                var p2txp1ty = p2t.X * (double) p1t.Y;
                var p2typ1tx = p2t.Y * (double) p1t.X;
                e0 = (float) (p2typ1tx - p2txp1ty);
                var p0txp2ty = p0t.X * (double) p2t.Y;
                var p0typ2tx = p0t.Y * (double) p2t.X;
                e1 = (float) (p0typ2tx - p0txp2ty);
                var p1txp0ty = p1t.X * (double) p0t.Y;
                var p1typ0tx = p1t.Y * (double) p0t.X;
                e2 = (float) (p1typ0tx - p1txp0ty);
            }

            // Perform triangle edge and determinant tests
            if ((e0 < 0 || e1 < 0 || e2 < 0) && (e0 > 0 || e1 > 0 || e2 > 0))
            {
                return false;
            }

            var det = e0 + e1 + e2;
            if (det == 0)
            {
                return false;
            }

            // Compute scaled hit distance to triangle and test against ray 't' range
            p0t.Z *= Sz;
            p1t.Z *= Sz;
            p2t.Z *= Sz;
            var tScaled = e0 * p0t.Z + e1 * p1t.Z + e2 * p2t.Z;
            if (det < 0 && (tScaled >= 0 || tScaled < r.TMax * det))
            {
                return false;
            }

            if (det > 0 && (tScaled <= 0 || tScaled > r.TMax * det))
            {
                return false;
            }

            // Compute barycentric coordinates and 't' value for triangle intersection
            var invDet = 1 / det;
            var b0 = e0 * invDet;
            var b1 = e1 * invDet;
            var b2 = e2 * invDet;
            var t = tScaled * invDet;

            // Ensure that computed triangle 't' is conservatively greater than zero

            // Compute 'delta_z' term for triangle 't' error bounds
            var maxZt = MaxDimension(Abs(new Vector(p0t.Z, p1t.Z, p2t.Z)));
            var deltaZ = Gamma(3) * maxZt;

            // Compute 'delta_x' and 'delta_y' terms for triangle 't' error bounds
            var maxXt = MaxDimension(Abs(new Vector(p0t.X, p1t.X, p2t.X)));
            var maxYt = MaxDimension(Abs(new Vector(p0t.Y, p1t.Y, p2t.Y)));
            var deltaX = Gamma(5) * (maxXt + maxZt);
            var deltaY = Gamma(5) * (maxYt + maxZt);

            // Compute 'delta_e' term for triangle 't' error bounds
            var deltaE = 2 * (Gamma(2) * maxXt * maxYt + deltaY * maxXt + deltaX * maxYt);

            // Compute 'delta_t' term for triangle 't' error bounds and check _t_
            var maxE = MaxDimension(Abs(new Vector(e0, e1, e2)));
            var deltaT = 3 * (Gamma(3) * maxE * maxZt + deltaE * maxZt + deltaZ * maxE) *
                         Abs(invDet);
            if (t <= deltaT)
            {
                return false;
            }

            // Test shadow ray intersection against alpha texture, if present
            if (!testAlphaTexture || !_mesh.HasAlphaMask && !_mesh.HasShadowAlphaMask)
            {
                return true;
            }

            // Compute triangle partial derivatives
            var dpdu = Vectors.Zero;
            var dpdv = Vectors.Zero;
            GetUVs(out var uv);

            // Compute deltas for triangle partial derivatives
            var duv02 = uv[0] - uv[2];
            var duv12 = uv[1] - uv[2];
            var dp02 = p0 - p2;
            var dp12 = p1 - p2;
            var determinant = duv02.X * duv12.Y - duv02.Y * duv12.X;
            var degenerateUV = Abs(determinant) < 1e-8;
            if (!degenerateUV)
            {
                var invdet = 1f / determinant;
                dpdu = (duv12.Y * dp02 - duv02.Y * dp12) * invdet;
                dpdv = (-duv12.X * dp02 + duv02.X * dp12) * invdet;
            }

            if (degenerateUV || Cross(dpdu, dpdv).LengthSquared() == 0)
            {
                // Handle zero determinant for triangle partial derivative matrix
                var ng = Cross(p2 - p0, p1 - p0);
                if (ng.LengthSquared() == 0)
                {
                    return false;
                }

                (dpdu, dpdv) = OrthonormalPosZ((Normal) Cross(p2 - p0, p1 - p0).Normalize());
            }

            // Interpolate (u,v) parametric coordinates and hit point
            var pHit = Sum(b0 * p0, b1 * p1, +b2 * p2);
            var uvHit = Point2D.Sum(b0 * uv[0], b1 * uv[1], b2 * uv[2]);

            var isectLocal = new SurfaceInteraction();
            isectLocal.Initialize(pHit, Vectors.Zero, uvHit, -r.Direction, dpdu, dpdv,
                                  Normals.Zero, Normals.Zero, this);


            if (_mesh.HasAlphaMask && _mesh.AlphaMask.Evaluate(isectLocal) == 0)
            {
                return false;
            }

            if (_mesh.HasShadowAlphaMask &&
                _mesh.ShadowAlphaMask.Evaluate(isectLocal) == 0)
            {
                return false;
            }

            return true;
        }

        public override Interaction Sample(in Point2D u, out float pdf)
        {
            var b = Sampling.Utilities.UniformSampleTriangle(u);
            var s = _v.Span;
            var p0 = _mesh.P[s[0]];
            var p1 = _mesh.P[s[1]];
            var p2 = _mesh.P[s[2]];
            var it = new Interaction
            {
                P = Sum(b[0] * p0, b[1] * p1, (1f - b[0] - b[1]) * p2),
                N = (Normal) Cross(p1 - p0, p2 - p0).Normalize()
            };
            if (_mesh.N.Length > 0)
            {
                var ns = b[0] * _mesh.N[s[0]] + b[1] * _mesh.N[s[1]] + (1f - b[0] - b[1]) * _mesh.N[s[2]];
                it.N = Normal.FaceForward(it.N, ns);
            }
            else if (ReverseOrientation ^ TransformSwapsHandedness)
            {
                it.N *= -1f;
            }

            var pAbsSum = Sum(Abs(b[0] * p0), Abs(b[1] * p1), Abs((1f - b[0] - b[1]) * p2));
            it.PError = Gamma(6) * (Vector) pAbsSum;
            pdf = 1f / Area();
            return it;
        }

        private void GetUVs(out Point2D[] uv)
        {
            uv = new Point2D[3];
            if (_mesh.HasUV)
            {
                var s = _v.Span;
                uv[0] = _mesh.UV[s[0]];
                uv[1] = _mesh.UV[s[1]];
                uv[2] = _mesh.UV[s[2]];
            }
            else
            {
                uv[0] = new Point2D(0, 0);
                uv[1] = new Point2D(1, 0);
                uv[2] = new Point2D(1, 1);
            }
        }
    }
}