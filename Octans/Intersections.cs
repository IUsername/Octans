using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Octans
{
    public interface IIntersectionsBuilder
    {
        int Count { get; }
        void AddRange(in IIntersections intersections);
        void Add(in Intersection intersection);
        IIntersections ToIntersections();
    }

    public interface IIntersections
    {
        int Count { get; }
        Intersection this[int index] { get; }
        Intersection? Hit(bool isInShadow = false);
        Intersection[] ToSorted();
        void Return();
    }

    public readonly struct Intersections : IIntersections
    {
        private static readonly IIntersections EmptySingleton = new Intersections(Array.Empty<Intersection>());
        private readonly Intersection[] _list;

        private Intersections(Intersection a)
        {
            Count = 1;
            _list = ArrayPool<Intersection>.Shared.Rent(Count);
            _list[0] = a;
        }

        private Intersections(Intersection a, Intersection b)
        {
            Count = 2;
            _list = ArrayPool<Intersection>.Shared.Rent(Count);
            _list[0] = a;
            _list[1] = b;
        }

        private Intersections(params Intersection[] intersections)
        {
            Count = intersections.Length;
            _list = ArrayPool<Intersection>.Shared.Rent(Count);
            Array.Copy(intersections, _list, Count);
        }

        private Intersections(in IList<Intersection> intersections)
        {
            Count = intersections.Count;
            _list = ArrayPool<Intersection>.Shared.Rent(Count);
            for (var i = 0; i < Count; i++)
            {
                _list[i] = intersections[i];
            }
        }

        public Intersection? Hit(bool isInShadow)
        {
            if (Count == 0)
            {
                return null;
            }

            Intersection? min = null;
            for (var i = 0; i < Count; i++)
            {
                if (!(_list[i].T > 0))
                {
                    continue;
                }

                if (!min.HasValue || min.Value.T > _list[i].T)
                {
                    if (!isInShadow || _list[i].Geometry.Material.CastsShadows)
                    {
                        min = _list[i];
                    }
                }
            }

            return min;
        }

        public int Count { get; }

        public Intersection this[int index] => _list[index];

        public Intersection[] ToSorted()
        {
            var valid = AsSpan().ToArray();
            Array.Sort(valid);
            return valid;
        }

        public void Return()
        {
            ArrayPool<Intersection>.Shared.Return(_list);
        }

        private Span<Intersection> AsSpan()
        {
            Span<Intersection> span = _list;
            return span.Slice(0, Count);
        }

        public static IntersectionsBuilder Builder() => IntersectionsBuilder.Create();

        public static IIntersections Create(params Intersection[] intersection) => new Intersections(intersection);
        public static IIntersections Create(Intersection a) => new Intersections(a);
        public static IIntersections Create(Intersection a, Intersection b) => new Intersections(a, b);

        public static IIntersections Empty() => EmptySingleton;

        public class IntersectionsBuilder : IIntersectionsBuilder
        {
            private static readonly ObjectPool<IntersectionsBuilder> ObjectPool =
                new ObjectPool<IntersectionsBuilder>(() => new IntersectionsBuilder());

            private readonly List<Intersection> _list;

            internal IntersectionsBuilder()
            {
                _list = new List<Intersection>();
            }

            public void AddRange(in IIntersections intersections)
            {
                for (var i = 0; i < intersections.Count; i++)
                {
                    _list.Add(intersections[i]);
                }
            }

            public void Add(in Intersection intersection)
            {
                _list.Add(intersection);
            }

            public int Count => _list.Count;

            public IIntersections ToIntersections()
            {
                if (_list.Count == 0)
                {
                    ObjectPool.PutObject(this);
                    return Empty();
                }

                var intersections = new Intersections(_list);
                _list.Clear();
                ObjectPool.PutObject(this);
                return intersections;
            }

            public static IntersectionsBuilder Create() => ObjectPool.GetObject();
        }
    }


    public class ObjectPool<T>
    {
        private readonly Func<T> _generator;
        private readonly ConcurrentBag<T> _objects;

        public ObjectPool(Func<T> generator)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _objects = new ConcurrentBag<T>();
        }

        public T GetObject() => _objects.TryTake(out var item) ? item : _generator();

        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}