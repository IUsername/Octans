namespace Octans
{
    public interface IMaterial
    {
        void ComputeScatteringFunctions(SurfaceInteraction si,
                                        IObjectArena arena,
                                        TransportMode mode,
                                        bool allowMultipleLobes);
    }
}