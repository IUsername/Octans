using System.Collections.Generic;

namespace Octans
{
    internal static class IntersectionCalculations
    {
        private static readonly ObjectPool<RefractiveIndexOrderer> OrdererPool = new ObjectPool<RefractiveIndexOrderer>(() => new RefractiveIndexOrderer());

        public static (float N1, float N2) DetermineN1N2(in Intersection intersection, in IIntersections intersections)
        {
            var orderer = OrdererPool.GetObject();
            var (n1, n2) = orderer.Sort(in intersection, in intersections);
            OrdererPool.PutObject(orderer);
            return (n1, n2);
        }

        private class RefractiveIndexOrderer
        {
            private readonly List<Intersection> _containers = new List<Intersection>();

            public (float N1, float N2) Sort(in Intersection intersection, in IIntersections intersections)
            {
                var n1 = 1.0f;
                var n2 = 1.0f;
                _containers.Clear();
                for (var i = 0; i < intersections.Count; i++)
                {
                    var current = intersections[i];
                    var isCurrent = current == intersection;
                    if (isCurrent && _containers.Count > 0)
                    {
                        n1 = _containers[_containers.Count - 1].Geometry.Material.RefractiveIndex;
                    }

                    var removed = _containers.RemoveAll(c => ReferenceEquals(current.Geometry, c.Geometry));
                    if (removed == 0)
                    {
                        _containers.Add(current);
                    }

                    if (!isCurrent)
                    {
                        continue;
                    }

                    if (_containers.Count > 0)
                    {
                        n2 = _containers[_containers.Count - 1].Geometry.Material.RefractiveIndex;
                    }

                    break;
                }

                return (N1: n1, N2: n2);
            }
        }
    }
}