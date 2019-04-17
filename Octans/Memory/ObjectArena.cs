using System;
using System.Collections.Generic;

namespace Octans.Memory
{
    public sealed class ObjectArena : IObjectArena
    {
        private readonly Dictionary<Type, IObjectPool> _typedPool = new Dictionary<Type, IObjectPool>();

        public T Create<T>() where T : new()
        {
            if (_typedPool.TryGetValue(typeof(T), out var pool))
            {
                return ((ObjectPool<T>) pool).Create();
            }

            var newPool = new ObjectPool<T>();
            _typedPool.Add(typeof(T), newPool);
            return newPool.Create();
        }

        public void Reset()
        {
            foreach (var pool in _typedPool.Values)
            {
                pool.Reset();
            }
        }

        public void Clear()
        {
            foreach (var pool in _typedPool.Values)
            {
                pool.Clear();
            }
        }
    }
}