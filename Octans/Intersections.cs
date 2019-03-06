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

        public Intersections(params Intersection[] intersections) : this(intersections.AsEnumerable())
        {
        }

        private Intersections()
        {
            _sorted = Array.Empty<Intersection>();
        }

        public Intersections(IEnumerable<Intersection> intersections)
        {
            _sorted = intersections.OrderBy(i => i.T).ToArray();
        }

        public IEnumerator<Intersection> GetEnumerator() => _sorted.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _sorted.Length;

        public Intersection this[int index] => _sorted[index];
    }
}