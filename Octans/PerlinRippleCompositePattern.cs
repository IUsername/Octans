namespace Octans
{
    public class PerlinRippleCompositePattern : PatternBase
    {
        public IPattern BasePattern { get; }
        public float Perturbation { get; }

        public PerlinRippleCompositePattern(IPattern basePattern, float perturbation)
        {
            BasePattern = basePattern;
            Perturbation = perturbation;
        }

        public override Color LocalColorAt(Point localPoint)
        {
            var x = Perlin.Noise(localPoint.X, localPoint.Y, localPoint.Z);
            var y = Perlin.Noise(localPoint.Z, localPoint.X, localPoint.Y);
            var z = Perlin.Noise(localPoint.Y, localPoint.Z, localPoint.X);
            var offset = new Vector(x, y, z) * Perturbation;

            var global = Transform * (localPoint + offset); 
            var baseLocal = BasePattern.TransformInverse() * global;
            return BasePattern.LocalColorAt(baseLocal);
        }
    }
}