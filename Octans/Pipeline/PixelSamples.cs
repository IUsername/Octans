using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Octans.Pipeline
{
    internal class PixelSamples : ISharedPixelSamples
    {
        private readonly ConcurrentDictionary<SubPixel, Color> _shared = new ConcurrentDictionary<SubPixel, Color>();
        private readonly ObjectPool<IPixelSamples> _localPool;

        public PixelSamples()
        {
            _localPool = new ObjectPool<IPixelSamples>(LocalScopeFactory);
        }

        public Color GetOrAdd(in SubPixel sp, IPixelRenderer renderer)
        {
            return _shared.GetOrAdd(sp, p => renderer.Render(in p));
        }

        public void Reset()
        {
            // Does nothing
        }

        public IPixelSamples CreateLocalScope()
        {
            return _localPool.GetObject();
        }

        private ScopedPixelSamples LocalScopeFactory()
        {
            return new ScopedPixelSamples(this);
        }

        public void CloseLocalScope(IPixelSamples samples)
        {
            samples.Reset();
            _localPool.PutObject(samples);
        }

        internal class ScopedPixelSamples : IPixelSamples
        {
            private readonly IPixelSamples _shared;
            private readonly Dictionary<SubPixel, Color> _scoped = new Dictionary<SubPixel, Color>();

            public ScopedPixelSamples(IPixelSamples shared)
            {
                _shared = shared;
            }

            public Color GetOrAdd(in SubPixel sp, IPixelRenderer renderer)
            {
                if (_scoped.TryGetValue(sp, out var color))
                {
                    return color;
                }

                if (sp.Dx == 0 || sp.Dy == 0)
                {
                    return _shared.GetOrAdd(in sp, renderer);
                }

                color = renderer.Render(in sp);
                _scoped.Add(sp, color);
                return color;
            }

            public void Reset()
            {
                _scoped.Clear();
            }
        }
    }
}