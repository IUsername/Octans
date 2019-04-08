using System.Collections.Generic;

namespace Octans.Geometry
{
    internal static class SolidExtensions
    {
        public static bool Includes(this IGeometry a, IGeometry b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            var queue = new Queue<IGeometry>();
            queue.Enqueue(a);
            while (queue.TryDequeue(out var current))
            {
                switch (current)
                {
                    case Group g:
                        for (var i = 0; i < g.Children.Count; i++)
                        {
                            var checking = g.Children[i];
                            if (ReferenceEquals(b, checking))
                            {
                                return true;
                            }

                            switch (checking)
                            {
                                case Group _:
                                    queue.Enqueue(checking);
                                    break;
                                case ConstructiveSolid s:
                                    queue.Enqueue(s.Left);
                                    queue.Enqueue(s.Right);
                                    break;
                            }
                        }

                        break;
                    case ConstructiveSolid s:
                        queue.Enqueue(s.Left);
                        queue.Enqueue(s.Right);
                        break;
                    default:
                        if (ReferenceEquals(b, current))
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }
    }
}