namespace Octans.Pipeline
{
    public interface ISharedPixelSamples : IPixelSamples
    {
        /// <summary>
        /// Creates an isolated scope for tracking pixel information. This local scope is not thread-safe.
        /// </summary>
        /// <returns>An isolated local scope that also has access to the outer scope pixel information.</returns>
        IPixelSamples CreateLocalScope();
        void CloseLocalScope(IPixelSamples samples);
    }
}