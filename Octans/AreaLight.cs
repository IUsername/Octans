namespace Octans
{
    public class AreaLight : ILight
    {
        public AreaLight(Point corner, Vector uLen, int uSteps, Vector vLen, int vSteps, Color intensity)
        {
            Corner = corner;
            U = uLen / uSteps;
            USteps = uSteps;
            V = vLen / vSteps;
            VSteps = vSteps;
            Intensity = intensity;
            Samples = uSteps * vSteps;
            Position = corner + uLen * 0.5f + vLen * 0.5f;
        }

        public Point Corner { get; }
        public Vector U { get; }
        public int USteps { get; }
        public Vector V { get; }
        public int VSteps { get; }
        public Color Intensity { get; }

        public Point Position { get; }

        public int Samples { get; }

        public Point UVPoint(int u, int v) => Corner + U * (u+0.5f) + V * (v + 0.5f);
    }
}