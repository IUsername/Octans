using System.Diagnostics.Contracts;
using Octans.Sampling;
using static System.Single;

namespace Octans
{
    public class Ray
    {
        public static Ray Undefined = new Ray(new Point(), new Vector(), 0f);
        public readonly Vector Direction;
        public readonly Vector InverseDirection;
        public readonly Point Origin;

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
        Spectrum Tr(Ray ray, ISampler2 sampler);
    }

    public class RayDifferential : Ray
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