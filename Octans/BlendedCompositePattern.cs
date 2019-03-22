namespace Octans
{
    public class BlendedCompositePattern : PatternBase
    {
        public BlendedCompositePattern(IPattern a, IPattern b)
        {
            A = a;
            B = b;
        }

        public IPattern A { get; }
        public IPattern B { get; }

        public override Color LocalColorAt(in Point localPoint)
        {
            var aLocal = A.TransformInverse() * localPoint;
            var a = A.LocalColorAt(in aLocal);

            var bLocal = B.TransformInverse() * localPoint;
            var b = B.LocalColorAt(in bLocal);

            return (a + b) / 2f;
        }
    }
}