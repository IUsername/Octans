namespace Octans
{
    public readonly struct LocalFrame
    {
        private readonly Vector _s;
        private readonly Vector _t;
        private readonly Vector _n;

        public LocalFrame(in Vector worldN)
        {
            _n = worldN;
            (_s, _t) = MathFunction.OrthonormalVectorsPosZ(in worldN);
        }

        public Vector ToLocal(in Vector v)
        {
            var x = _s % v;
            var y = _t % v;
            var z = _n % v;
            return new Vector(x, y, z);
        }

        public Vector ToWorld(in Vector v)
        {
            return _s * v.X + _t * v.Y + _n * v.Z;
        }
    }
}