using Octans.Memory;
using Octans.Reflection;

namespace Octans.Material
{
    public interface IMaterial
    {
        void ComputeScatteringFunctions(SurfaceInteraction si,
                                        IObjectArena arena,
                                        TransportMode mode,
                                        bool allowMultipleLobes);
    }
}