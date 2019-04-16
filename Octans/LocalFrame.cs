namespace Octans
{
    public readonly struct LocalFrame
    {
        private readonly Vector _s;
        private readonly Vector _t;
        private readonly Vector _n;

        public LocalFrame(in Normal worldN)
        {
            _n = (Vector)worldN;
            (_s, _t) = MathF.OrthonormalPosZ(in worldN);
        }

        public Normal ToLocal(in Normal n)
        {
            var x = _s % n;
            var y = _t % n;
            var z = _n % n;
            return new Normal(x, y, z);
        }

        public Vector ToLocal(in Vector v)
        {
            var x = _s % v;
            var y = _t % v;
            var z = _n % v;
            return new Vector(x, y, z);
        }

        public Normal ToWorld(in Normal v)
        {
            return (Normal)( _s * v.X + _t * v.Y + _n * v.Z);
        }

        public Vector ToWorld(in Vector v)
        {
            return _s * v.X + _t * v.Y + _n * v.Z;
        }
    }
}