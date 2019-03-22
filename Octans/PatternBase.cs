namespace Octans
{
    public abstract class PatternBase : IPattern
    {
        private Matrix _inverse;
        private Matrix _transform;

        protected PatternBase()
        {
            Transform = Matrix.Identity;
            _inverse = Matrix.Identity;
        }

        public Matrix Transform
        {
            get => _transform;
            protected set
            {
                _transform = value;
                _inverse = value.Inverse();
            }
        }

        public Matrix TransformInverse() => _inverse;

        public abstract Color LocalColorAt(in Point localPoint);

        public void SetTransform(Matrix transform)
        {
            Transform = transform;
        }
    }
}