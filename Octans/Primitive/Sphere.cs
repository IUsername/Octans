using static System.MathF;
using static Octans.MathF;

namespace Octans.Primitive
{
    public class Sphere : Shape
    {
        public Sphere(Transform objectToWorld,
                      Transform worldToObject,
                      bool reverseOrientation,
                      float radius,
                      float zMin,
                      float zMax,
                      float phiMax) : base(objectToWorld, worldToObject, reverseOrientation)
        {
            Radius = radius;
            ZMin = Clamp(-Radius, Radius, Min(zMin, zMax));
            ZMax = Clamp(-Radius, Radius, Max(zMin, zMax));
            ThetaMin = Acos(Clamp(-1, 1, Min(zMin, zMax) / Radius));
            ThetaMax = Acos(Clamp(-1, 1, Max(zMin, zMax) / Radius));
            PhiMax = Rad(Clamp(0, 360, phiMax));
        }

        public float ThetaMax { get; }

        public float ThetaMin { get; }

        public float Radius { get; }
        public float ZMin { get; }
        public float ZMax { get; }
        public float PhiMax { get; }

        //public override Interaction Sample(Point2D u, out float pdf) => throw new System.NotImplementedException();

        public override Bounds ObjectBounds =>
            new Bounds(new Point(-Radius, -Radius, ZMin), new Point(Radius, Radius, ZMax));

        public override float Area() => PhiMax * Radius * (ZMax - ZMin);

        public override bool Intersect(in Ray r,
                                       out float tHit,
                                       ref SurfaceInteraction si,
                                       bool testAlphaTexture = true)
        {
            tHit = 0f;

            var (ray, oErr, dErr) = WorldToObject & r;

            var ox = new EFloat(ray.Origin.X, oErr.X);
            var oy = new EFloat(ray.Origin.Y, oErr.Y);
            var oz = new EFloat(ray.Origin.Z, oErr.Z);
            var dx = new EFloat(ray.Direction.X, dErr.X);
            var dy = new EFloat(ray.Direction.Y, dErr.Y);
            var dz = new EFloat(ray.Direction.Z, dErr.Z);

            var a = dx * dx + dy * dy + dz * dz;
            var b = 2 * (dx * ox + dy * oy + dz * oz);
            var c = ox * ox + oy * oy + oz * oz - (EFloat) Radius * (EFloat) Radius;

            if (!EFloat.Quadratic(a, b, c, out var t0, out var t1))
            {
                return false;
            }

            if (t0.UpperBound() > ray.TMax || t1.LowerBound() <= 0f)
            {
                return false;
            }

            var tShapeHit = t0;
            if (tShapeHit.LowerBound() <= 0f)
            {
                tShapeHit = t1;
                if (tShapeHit.UpperBound() > ray.TMax)
                {
                    return false;
                }
            }

            var pHit = ray.Position((float) tShapeHit);
            pHit *= Radius / Point.Distance(pHit, Point.Zero);
            if (pHit.X == 0f && pHit.Y == 0f)
            {
                pHit = new Point(1e-5f * Radius, pHit.Y, pHit.Z);
            }

            var phi = Atan2(pHit.Y, pHit.X);
            if (phi < 0f)
            {
                phi += 2 * PI;
            }

            if (ZMin > -Radius && pHit.Z < ZMin || ZMax < Radius && pHit.Z > ZMax || phi > PhiMax)
            {
                if (tShapeHit == t1)
                {
                    return false;
                }

                if (t1.UpperBound() > ray.TMax)
                {
                    return false;
                }

                tShapeHit = t1;

                pHit = ray.Position((float) tShapeHit);
                pHit *= Radius / Point.Distance(pHit, Point.Zero);
                if (pHit.X == 0f && pHit.Y == 0f)
                {
                    pHit = new Point(1e-5f * Radius, pHit.Y, pHit.Z);
                }

                phi = Atan2(pHit.Y, pHit.X);
                if (phi < 0f)
                {
                    phi += 2 * PI;
                }

                if (ZMin > -Radius && pHit.Z < ZMin || ZMax < Radius && pHit.Z > ZMax || phi > PhiMax)
                {
                    return false;
                }
            }

            // Find parametric representation
            var u = phi / PhiMax;
            var theta = Acos(Clamp(-1, 1, pHit.Z / Radius));
            var v = (theta - ThetaMin) / (ThetaMax - ThetaMin);

            var zRadius = Sqrt(pHit.X * pHit.X + pHit.Y * pHit.Y);
            var invZRadius = 1f / zRadius;
            var cosPhi = pHit.X * invZRadius;
            var sinPhi = pHit.Y * invZRadius;
            var dpdu = new Vector(-PhiMax * pHit.Y, PhiMax * pHit.X, 0f);
            var dpdv = (ThetaMax - ThetaMin) * new Vector(pHit.Z * cosPhi, pHit.Z * sinPhi, -Radius * Sin(theta));

            var d2Pduu = -PhiMax * PhiMax * new Vector(pHit.X, pHit.Y, 0f);
            var d2Pduv = (ThetaMax - ThetaMin) * pHit.Z * PhiMax * new Vector(-sinPhi, cosPhi, 0f);
            var d2Pdvv = -(ThetaMax - ThetaMin) * (ThetaMax - ThetaMin) * new Vector(pHit.X, pHit.Y, pHit.Z);

            var E = dpdu % dpdu;
            var F = dpdu % dpdv;
            var G = dpdv % dpdv;
            var N = Vector.Cross(dpdu, dpdv).Normalize();
            var e = N % d2Pduu;
            var f = N % d2Pduv;
            var g = N % d2Pdvv;

            var invEGF2 = 1f / (E * G - F * F);
            var dndu = (Normal) ((f * F - e * G) * invEGF2 * dpdu +
                                 (e * F - f * E) * invEGF2 * dpdv);

            var dndv = (Normal) ((g * F - f * G) * invEGF2 * dpdu +
                                 (f * F - g * E) * invEGF2 * dpdv);

            var pError = Gamma(5) * Vector.Abs((Vector) pHit);

            si = ObjectToWorld *
                 si.Initialize(pHit, pError, new Point2D(u, v), -ray.Direction, dpdu, dpdv, dndu, dndv, this);

            tHit = (float) tShapeHit;
            return true;
        }


        public override bool IntersectP(in Ray r, bool testAlphaTexture = true)
        {
            var (ray, oErr, dErr) = WorldToObject & r;

            var ox = new EFloat(ray.Origin.X, oErr.X);
            var oy = new EFloat(ray.Origin.Y, oErr.Y);
            var oz = new EFloat(ray.Origin.Z, oErr.Z);
            var dx = new EFloat(ray.Direction.X, dErr.X);
            var dy = new EFloat(ray.Direction.Y, dErr.Y);
            var dz = new EFloat(ray.Direction.Z, dErr.Z);

            var a = dx * dx + dy * dy + dz * dz;
            var b = 2 * (dx * ox + dy * oy + dz * oz);
            var c = ox * ox + oy * oy + oz * oz - (EFloat)Radius * (EFloat)Radius;

            if (!EFloat.Quadratic(a, b, c, out var t0, out var t1))
            {
                return false;
            }

            if (t0.UpperBound() > ray.TMax || t1.LowerBound() <= 0f)
            {
                return false;
            }

            var tShapeHit = t0;
            if (tShapeHit.LowerBound() <= 0f)
            {
                tShapeHit = t1;
                if (tShapeHit.UpperBound() > ray.TMax)
                {
                    return false;
                }
            }

            var pHit = ray.Position((float)tShapeHit);
            pHit *= Radius / Point.Distance(pHit, Point.Zero);
            if (pHit.X == 0f && pHit.Y == 0f)
            {
                pHit = new Point(1e-5f * Radius, pHit.Y, pHit.Z);
            }

            var phi = Atan2(pHit.Y, pHit.X);
            if (phi < 0f)
            {
                phi += 2 * PI;
            }

            if (ZMin > -Radius && pHit.Z < ZMin || ZMax < Radius && pHit.Z > ZMax || phi > PhiMax)
            {
                if (tShapeHit == t1)
                {
                    return false;
                }

                if (t1.UpperBound() > ray.TMax)
                {
                    return false;
                }

                tShapeHit = t1;

                pHit = ray.Position((float)tShapeHit);
                pHit *= Radius / Point.Distance(pHit, Point.Zero);
                if (pHit.X == 0f && pHit.Y == 0f)
                {
                    pHit = new Point(1e-5f * Radius, pHit.Y, pHit.Z);
                }

                phi = Atan2(pHit.Y, pHit.X);
                if (phi < 0f)
                {
                    phi += 2 * PI;
                }

                if (ZMin > -Radius && pHit.Z < ZMin || ZMax < Radius && pHit.Z > ZMax || phi > PhiMax)
                {
                    return false;
                }
            }

            return true;
        }
    }
}