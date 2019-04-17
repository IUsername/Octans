using System.Collections.Generic;

namespace Octans.Memory
{
    public sealed class ObjectPool<T> : IObjectPool where T : new()
    {
        private readonly Queue<T> _active = new Queue<T>();
        private readonly Queue<T> _used = new Queue<T>();

        public void Reset()
        {
            while (_active.TryDequeue(out var a))
            {
                _used.Enqueue(a);
            }
        }

        public void Clear()
        {
            _active.Clear();
            _used.Clear();
        }

        public T Create()
        {
            var instance = _used.TryDequeue(out var result) ? result : new T();
            _active.Enqueue(instance);
            return instance;
        }
    }
}