namespace Octans
{
    public sealed class CameraPosition : ICameraPosition
    {
        public CameraPosition(Matrix transform)
        {
            Transform = transform;
            TransformInverse = transform.Inverse();
        }

        public Matrix Transform { get; }
        public Matrix TransformInverse { get; }
    }
}