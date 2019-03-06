using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Octans
{
    public class Intersections : IReadOnlyList<Intersection>
    {
        public static Intersections Empty = new Intersections();
        private readonly Intersection[] _sorted;

        public Intersections(params Intersection[] intersections)
        {
            _sorted = intersections;
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Array.Sort(_sorted, (a, b) => a.T.CompareTo(b.T));
        }

        private Intersections()
        {
            _sorted = Array.Empty<Intersection>();
        }

        public IEnumerator<Intersection> GetEnumerator() => _sorted.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _sorted.Length;

        public Intersection this[int index] => _sorted[index];
    }
}