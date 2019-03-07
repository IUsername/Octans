namespace Octans
{
    public abstract class PatternBase : IPattern
    {
        protected PatternBase()
        {
            Transform = Matrix.Identity;
        }

        public Matrix Transform { get; protected set; }
        public abstract Color LocalColorAt(Point localPoint);

        public void SetTransform(Matrix transform)
        {
            Transform = transform;
        }
    }
}