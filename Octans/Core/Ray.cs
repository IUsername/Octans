using System.Diagnostics.Contracts;
using Octans.Sampling;
using static System.Single;

namespace Octans
{
    public class Ray
    {
        public static Ray Undefined = new Ray(new Point(), new Vector(), 0f);
        public Vector Direction { get; set; }
        public Vector InverseDirection { get; set; }
        public Point Origin { get; set; }

        public Ray(Point origin, Vector direction, float tMax = PositiveInfinity)
        {
            TMax = tMax;
            Origin = origin;
            Direction = direction;
            InverseDirection = 1f / direction;
        }

        public float TMax { get; set; }
        public IMedium Medium { get; set; }

        public Point Position(float t) => Origin + Direction * t;
    }

    public interface IMedium
    {
        Spectrum Tr(Ray ray, ISampler sampler);
    }

    public sealed class RayDifferential : Ray
    {
        public new static RayDifferential Undefined = new RayDifferential(new Point(), new Vector(), 0f);

        public RayDifferential(Point origin, Vector direction, float tMax = PositiveInfinity) : base(
            origin, direction, tMax)
        {
            HasDifferentials = false;
        }

        public RayDifferential(Ray ray) : this(ray.Origin, ray.Direction, ray.TMax)
        {
        }

        public RayDifferential() : base(Point.Zero, Vectors.Zero)
        {
            HasDifferentials = false;
        }

        public RayDifferential Initialize(in Ray ray)
        {
            HasDifferentials = false;
            base.Origin = ray.Origin;
            base.Direction = ray.Direction;
            base.InverseDirection = ray.InverseDirection;
            TMax = PositiveInfinity;
            return this;
        }

        public RayDifferential Initialize(in Point p, in Vector d)
        {
            HasDifferentials = false;
            base.Origin = p;
            base.Direction = d;
            base.InverseDirection = 1f / d;
            TMax = PositiveInfinity;
            return this;
        }

        public bool HasDifferentials { get; set; }

        public Point RxOrigin { get; set; }

        public Point RyOrigin { get; set; }

        public Vector RxDirection { get; set; }

        public Vector RyDirection { get; set; }

        public void ScaleDifferentials(float s)
        {
            RxOrigin = Origin + (RxOrigin - Origin) * s;
            RyOrigin = Origin + (RyOrigin - Origin) * s;
            RxDirection = Direction + (RxDirection - Direction) * s;
            RyDirection = Direction + (RyDirection - Direction) * s;
        }
    }
}