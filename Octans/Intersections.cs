using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Octans
{
    public interface IIntersectionsBuilder
    {
        int Count { get; }
        void AddRange(IEnumerable<Intersection> intersections);
        void Add(Intersection intersection);
        IIntersections ToIntersections();
    }

    public interface IIntersections : IEnumerable<Intersection>
    {
        int Count { get; }
        Intersection this[int index] { get; }
        Intersection? Hit(bool isInShadow=false);
        Intersection[] ToSorted();
    }

    public class Intersections : IIntersections
    {
        private readonly ImmutableList<Intersection> _list;

        private Intersections(IEnumerable<Intersection> intersections)
        {
            _list = intersections.ToImmutableList();
        }

        private Intersections(params Intersection[] intersections)
        {
            _list = intersections.ToImmutableList();
        }

        private Intersections(ImmutableList<Intersection> intersections)
        {
            _list = intersections;
        }

        public Intersection? Hit(bool isInShadow)
        {
            if (_list.IsEmpty)
            {
                return null;
            }
            
            Intersection? min = null;
            for (var i = 0; i < _list.Count; i++)
            {
                if (!(_list[i].T > 0))
                {
                    continue;
                }

                if (!min.HasValue || min.Value.T > _list[i].T)
                {
                    if (!isInShadow || _list[i].Shape.Material.CastsShadows)
                    {
                        min = _list[i];
                    }
                }
            }

            return min;
        }

        public int Count => _list.Count;

        public Intersection this[int index] => _list[index];
        public Intersection[] ToSorted() => _list.Sort().ToArray();

        public static IntersectionsBuilder Builder() => IntersectionsBuilder.Create();
        //public static IntersectionsBuilder Builder()
        //{
        //    return new IntersectionsBuilder();
        //}

        public static IIntersections Create(params Intersection[] intersection) => new Intersections(intersection);

        private static readonly IIntersections EmptySingleton = new Intersections(ImmutableList<Intersection>.Empty);

        public static IIntersections Empty() => EmptySingleton;

        public class IntersectionsBuilder : IIntersectionsBuilder
        {
            private static readonly ObjectPool<IntersectionsBuilder> Pool =
                new ObjectPool<IntersectionsBuilder>(() => new IntersectionsBuilder());

            private readonly List<Intersection> _list;

            internal IntersectionsBuilder()
            {
                _list = new List<Intersection>();
            }

            public void AddRange(IEnumerable<Intersection> intersections)
            {
                _list.AddRange(intersections);
            }

            public void Add(Intersection intersection)
            {
                _list.Add(intersection);
            }

            public int Count => _list.Count;

            public IIntersections ToIntersections()
            {
                if (_list.Count == 0)
                {
                    Pool.PutObject(this);
                    return Empty();
                }

                var intersections = new Intersections(_list);
                _list.Clear();
                Pool.PutObject(this);
                return intersections;
            }

            public static IntersectionsBuilder Create() => Pool.GetObject();
        }

        public IEnumerator<Intersection> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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