using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octans.Memory;
using Octans.Primitive;

namespace Octans.Accelerator
{
    public sealed class BVH : IPrimitive
    {
        private readonly LinearBVHNode[] _nodes;
        private readonly IPrimitive[] _p;
        private readonly object _padlock = new object();

        public BVH(IPrimitive[] p, SplitMethod splitMethod, int maxPerNode = 1)
        {
            _p = p;
            SplitMethod = splitMethod;
            MaxPerNode = maxPerNode;

            var primitiveInfo = new BVHPrimitiveInfo[p.Length];
            for (var i = 0; i < primitiveInfo.Length; ++i)
            {
                primitiveInfo[i] = new BVHPrimitiveInfo(i, p[i].WorldBounds);
            }

            var ordered = new List<IPrimitive>(p);
            var arena = new ObjectArena();
            var totalNodes = 0;

            var root = splitMethod == SplitMethod.HLBVH
                ? HLBVHBuild(arena, primitiveInfo, out totalNodes, ordered)
                : RecursiveBuild(arena, primitiveInfo, 0, p.Length, ref totalNodes, ordered);

            _p = ordered.ToArray();

            _nodes = new LinearBVHNode[totalNodes];

            var offset = 0;
            FlattenBVHTree(root, ref offset);
        }

        public SplitMethod SplitMethod { get; }

        public int MaxPerNode { get; }

        public Bounds WorldBounds => _nodes.Length > 0 ? _nodes[0].Bounds : Bounds.Empty;

        public bool Intersect(ref Ray r, ref SurfaceInteraction si)
        {
            if (_nodes.Length == 0)
            {
                return false;
            }

            var hit = false;
            var dirIsNeg = DirIsNeg(r);
            var toVisitOffset = 0;
            var currentNodeIndex = 0;
            var nodesToVisit = new int[64];
            while (true)
            {
                var node = _nodes[currentNodeIndex];
                if (node.Bounds.IntersectP(r, dirIsNeg))
                {
                    if (node.NPrimitives > 0)
                    {
                        for (var i = 0; i < node.NPrimitives; ++i)
                        {
                            if (_p[node.PrimitivesOffset + i].Intersect(ref r, ref si))
                            {
                                hit = true;
                            }
                        }

                        if (toVisitOffset == 0)
                        {
                            break;
                        }

                        currentNodeIndex = nodesToVisit[--toVisitOffset];
                    }
                    else
                    {
                        if (dirIsNeg[node.Axis] == 1)
                        {
                            nodesToVisit[toVisitOffset++] = currentNodeIndex + 1;
                            currentNodeIndex = node.SecondChildOffset;
                        }
                        else
                        {
                            nodesToVisit[toVisitOffset++] = node.SecondChildOffset;
                            currentNodeIndex += 1;
                        }
                    }
                }
                else
                {
                    if (toVisitOffset == 0)
                    {
                        break;
                    }

                    currentNodeIndex = nodesToVisit[--toVisitOffset];
                }
            }

            return hit;
        }

        public bool IntersectP(ref Ray r)
        {
            if (_nodes.Length == 0)
            {
                return false;
            }

            var dirIsNeg = DirIsNeg(r);
            var toVisitOffset = 0;
            var currentNodeIndex = 0;
            var nodesToVisit = new int[64];
            while (true)
            {
                var node = _nodes[currentNodeIndex];
                if (node.Bounds.IntersectP(r, dirIsNeg))
                {
                    if (node.NPrimitives > 0)
                    {
                        for (var i = 0; i < node.NPrimitives; ++i)
                        {
                            if (_p[node.PrimitivesOffset + i].IntersectP(ref r))
                            {
                                return true;
                            }
                        }

                        if (toVisitOffset == 0)
                        {
                            break;
                        }

                        currentNodeIndex = nodesToVisit[--toVisitOffset];
                    }
                    else
                    {
                        if (dirIsNeg[node.Axis] == 1)
                        {
                            nodesToVisit[toVisitOffset++] = currentNodeIndex + 1;
                            currentNodeIndex = node.SecondChildOffset;
                        }
                        else
                        {
                            nodesToVisit[toVisitOffset++] = node.SecondChildOffset;
                            currentNodeIndex += 1;
                        }
                    }
                }
                else
                {
                    if (toVisitOffset == 0)
                    {
                        break;
                    }

                    currentNodeIndex = nodesToVisit[--toVisitOffset];
                }
            }

            return false;
        }

        public IMaterial Material { get; }

        public IAreaLight AreaLight { get; }

        public void ComputeScatteringFunctions(SurfaceInteraction surfaceInteraction,
                                               IObjectArena arena,
                                               TransportMode mode,
                                               in bool allowMultipleLobes)
        {
            throw new NotImplementedException();
        }

        private static int[] DirIsNeg(Ray r)
        {
            var dirIsNeg = new[]
            {
                r.InverseDirection.X < 0f ? 1 : 0,
                r.InverseDirection.Y < 0f ? 1 : 0,
                r.InverseDirection.Z < 0f ? 1 : 0
            };
            return dirIsNeg;
        }

        private int FlattenBVHTree(BVHBuildNode node, ref int offset)
        {
            ref var linearNode = ref _nodes[offset];
            linearNode.Bounds = node.Bounds;
            var myOffset = offset++;
            if (node.NPrimitives > 0)
            {
                linearNode.PrimitivesOffset = node.FirstPrimitiveOffset;
                linearNode.NPrimitives = (ushort) node.NPrimitives;
            }
            else
            {
                linearNode.Axis = (byte) node.SplitAxis;
                linearNode.NPrimitives = 0;
                FlattenBVHTree(node.Children[0], ref offset);
                linearNode.SecondChildOffset = FlattenBVHTree(node.Children[1], ref offset);
            }

            return myOffset;
        }

        private BVHBuildNode HLBVHBuild(ObjectArena arena,
                                        BVHPrimitiveInfo[] primitiveInfo,
                                        out int totalNodes,
                                        IList<IPrimitive> ordered)
        {
            var bounds = primitiveInfo.Aggregate(Bounds.Empty, (current, t) => current + t.Bounds);

            var mortonPrims = new MortonPrimitive[primitiveInfo.Length];
            Parallel.For(0, primitiveInfo.Length, i =>
            {
                const int mortonBits = 10;
                const int mortonScale = 1 << mortonBits;
                mortonPrims[i].PrimitiveIndex = primitiveInfo[i].PrimitiveNumber;
                var centroidOffset = bounds.Offset(primitiveInfo[i].Centroid);
                mortonPrims[i].MortonCode = (centroidOffset * mortonScale).EncodeMorton3();
            });

            mortonPrims.RadixSort();
            var treeletsToBuild = new List<LBVHTreelet>();
            var start = 0;
            var end = 1;
            for (; end <= mortonPrims.Length; ++end)
            {
                const uint mask = 0b00111111111111000000000000000000;
                if (end == mortonPrims.Length || (mortonPrims[start].MortonCode & mask) !=
                    (mortonPrims[end].MortonCode & mask))
                {
                    var nPrimitives = end - start;
                    var maxBVHNodes = 2 * nPrimitives;
                    var nodes = new BVHBuildNode[maxBVHNodes];
                    treeletsToBuild.Add(new LBVHTreelet
                    {
                        StartIndex = start,
                        NPrimitives = nPrimitives,
                        BuildNodes = nodes
                    });
                    start = end;
                }
            }

            var atomicTotal = 0;
            var orderedPrimsOffset = 0;
            var treelets = treeletsToBuild.ToArray();
            Parallel.For(0, treelets.Length, i =>
            {
                var nodesCreated = 0;
                const int firstBitIndex = 29 - 12;
                var span = new Span<MortonPrimitive>(mortonPrims);
                ref var tr = ref treelets[i];
                tr.BuildNodes = new[]
                {
                    EmitLBVH(new Queue<BVHBuildNode>(tr.BuildNodes), primitiveInfo, span.Slice(tr.StartIndex),
                             tr.NPrimitives,
                             ref nodesCreated, ordered, ref orderedPrimsOffset, firstBitIndex)
                };
                Interlocked.Add(ref atomicTotal, nodesCreated);
            });
            totalNodes = atomicTotal;

            var finishedTreelets = treelets.Select(t => t.BuildNodes.First()).ToArray();

            return BuildUpperSAH(arena, finishedTreelets, 0, finishedTreelets.Length, ref totalNodes);
        }

        private static BVHBuildNode BuildUpperSAH(ObjectArena arena,
                                                  BVHBuildNode[] treeletRoots,
                                                  int start,
                                                  in int end,
                                                  ref int totalNodes)
        {
            var nNodes = end - start;
            if (nNodes == 1)
            {
                return treeletRoots[start];
            }

            totalNodes++;
            var node = arena.Create<BVHBuildNode>();

            var bounds = Bounds.Empty;
            for (var i = start; i < end; ++i)
            {
                bounds += treeletRoots[i].Bounds;
            }

            var centroidBounds = Bounds.Empty;
            for (var i = start; i < end; ++i)
            {
                var centroid = treeletRoots[i].Bounds.Centroid;
                centroidBounds += centroid;
            }

            var dim = centroidBounds.MaximumExtent();

            const int nBuckets = 12;
            var buckets = new BucketInfo[nBuckets];

            for (var i = start; i < end; ++i)
            {
                var centroid = (treeletRoots[i].Bounds.Min[dim] + treeletRoots[i].Bounds.Max[dim]) * 0.5f;
                var b = (int) (nBuckets * ((centroid - centroidBounds.Min[dim]) /
                                           (centroidBounds.Max[dim] - centroidBounds.Min[dim])));
                if (b == nBuckets)
                {
                    b = nBuckets - 1;
                }

                buckets[b].Count++;
                buckets[b].Bounds += treeletRoots[i].Bounds;
            }

            var cost = new float[nBuckets - 1];
            for (var i = 0; i < nBuckets - 1; ++i)
            {
                var b0 = Bounds.Empty;
                var b1 = Bounds.Empty;
                var count0 = 0;
                var count1 = 0;
                for (var j = 0; j <= i; ++j)
                {
                    b0 += buckets[j].Bounds;
                    count0 += buckets[j].Count;
                }

                for (var j = i + 1; j < nBuckets; ++j)
                {
                    b1 += buckets[j].Bounds;
                    count1 += buckets[j].Count;
                }

                cost[i] = 0.125f + (count0 * b0.SurfaceArea() + count1 * b1.SurfaceArea()) / bounds.SurfaceArea();
            }

            var minCost = cost[0];
            var minCostSplitBucket = 0;
            for (var i = 1; i < nBuckets - 1; ++i)
            {
                if (cost[i] < minCost)
                {
                    minCost = cost[i];
                    minCostSplitBucket = i;
                }
            }

            var mid = treeletRoots.Partition(start, end, buildNode =>
            {
                var centroid = (buildNode.Bounds.Min[dim] + buildNode.Bounds.Max[dim]) * 0.5f;
                var b = (int) (nBuckets * ((centroid - centroidBounds.Min[dim]) /
                                           (centroidBounds.Max[dim] - centroidBounds.Min[dim])));
                if (b == nBuckets)
                {
                    b = nBuckets - 1;
                }
                Debug.Assert(b >= 0);
                Debug.Assert(b < nBuckets);

                return b <= minCostSplitBucket;
            });
           
            Debug.Assert(mid > start);
            Debug.Assert(mid < end);

            node.InitInterior(dim,
                              BuildUpperSAH(arena, treeletRoots, start, mid, ref totalNodes),
                              BuildUpperSAH(arena, treeletRoots, mid, end, ref totalNodes));
            return node;
        }

        private BVHBuildNode EmitLBVH(Queue<BVHBuildNode> buildNodes,
                                      BVHPrimitiveInfo[] primitiveInfo,
                                      Span<MortonPrimitive> mortonPrims,
                                      in int nPrimitives,
                                      ref int totalNodes,
                                      IList<IPrimitive> ordered,
                                      ref int orderedPrimsOffset,
                                      in int bitIndex)
        {
            if (bitIndex == -1 || nPrimitives < MaxPerNode)
            {
                totalNodes++;
                var node = buildNodes.Dequeue();
                var bounds = Bounds.Empty;
                int firstPrimeOffset;
                lock (_padlock)
                {
                    firstPrimeOffset = orderedPrimsOffset;
                    Interlocked.Add(ref orderedPrimsOffset, nPrimitives);
                }

                for (var i = 0; i < nPrimitives; ++i)
                {
                    var primitiveIndex = mortonPrims[i].PrimitiveIndex;
                    ordered[firstPrimeOffset + i] = _p[primitiveIndex];
                    bounds += primitiveInfo[primitiveIndex].Bounds;
                }

                node.InitLeaf(firstPrimeOffset, nPrimitives, bounds);
                return node;
            }
            else
            {
                var mask = 1 << bitIndex;
                if ((mortonPrims[0].MortonCode & mask) ==
                    (mortonPrims[nPrimitives - 1].MortonCode & mask))
                {
                    return EmitLBVH(buildNodes, primitiveInfo, mortonPrims, nPrimitives, ref totalNodes, ordered,
                                    ref orderedPrimsOffset, bitIndex - 1);
                }

                var searchStart = 0;
                var searchEnd = nPrimitives - 1;
                while (searchStart + 1 != searchEnd)
                {
                    var mid = (searchStart + searchEnd) / 2;
                    if ((mortonPrims[searchStart].MortonCode & mask) ==
                        (mortonPrims[mid].MortonCode & mask))
                    {
                        searchStart = mid;
                    }
                    else
                    {
                        searchEnd = mid;
                    }
                }

                var splitOffset = searchEnd;
                totalNodes++;
                var node = buildNodes.Dequeue();

                var a = EmitLBVH(buildNodes, primitiveInfo, mortonPrims, splitOffset, ref totalNodes, ordered,
                                 ref orderedPrimsOffset, bitIndex - 1);

                var b = EmitLBVH(buildNodes, primitiveInfo, mortonPrims.Slice(splitOffset), nPrimitives - splitOffset,
                                 ref totalNodes, ordered,
                                 ref orderedPrimsOffset, bitIndex - 1);

                var axis = bitIndex % 3;
                node.InitInterior(axis, a, b);
                return node;
            }
        }


        private BVHBuildNode RecursiveBuild(ObjectArena arena,
                                            BVHPrimitiveInfo[] primitiveInfo,
                                            int start,
                                            in int end,
                                            ref int totalNodes,
                                            ICollection<IPrimitive> ordered)
        {
            totalNodes++;
            var node = arena.Create<BVHBuildNode>();
            var bounds = Bounds.Empty;
            for (var i = start; i < end; ++i)
            {
                bounds += primitiveInfo[i].Bounds;
            }

            var nPrimitives = end - start;
            if (nPrimitives == 1)
            {
                var startOffset = ordered.Count;
                for (var i = start; i < end; ++i)
                {
                    var n = primitiveInfo[i].PrimitiveNumber;
                    ordered.Add(_p[n]);
                }

                node.InitLeaf(startOffset, nPrimitives, bounds);
                return node;
            }

            var centroidBounds = Bounds.Empty;
            for (var i = start; i < end; ++i)
            {
                centroidBounds += primitiveInfo[i].Centroid;
            }

            var dim = centroidBounds.MaximumExtent();
            var mid = (start + end) / 2;
            if (centroidBounds.Max[dim] == centroidBounds.Min[dim])
            {
                var startOffset = ordered.Count;
                for (var i = start; i < end; ++i)
                {
                    var n = primitiveInfo[i].PrimitiveNumber;
                    ordered.Add(_p[n]);
                }

                node.InitLeaf(startOffset, nPrimitives, bounds);
                return node;
            }

            static Comparison<BVHPrimitiveInfo> Comp(int dim)
            {
                return (a, b) => a.Centroid[dim].CompareTo(b.Centroid[dim]);
            }

            switch (SplitMethod)
            {
                case SplitMethod.Middle:
                    var pMid = (centroidBounds.Min[dim] + centroidBounds.Max[dim]) / 2f;
                    mid = primitiveInfo.Partition(start, end, i => i.Centroid[dim] < pMid);
                    if (mid == start || mid == end)
                    {
                        mid = (start + end) / 2;
                        primitiveInfo.NthElement(start, end, mid, Comp(dim));
                    }

                    break;
                case SplitMethod.EqualCounts:
                    //mid = (start + end) / 2;
                    primitiveInfo.NthElement(start, end, mid, Comp(dim));
                    break;
                case SplitMethod.SAH:
                case SplitMethod.HLBVH:
                default:
                    if (nPrimitives <= 2)
                    {
                        //mid = (start + end) / 2;
                        primitiveInfo.NthElement(start, end, mid, Comp(dim));
                    }
                    else
                    {
                        var nBuckets = 12;
                        var buckets = new BucketInfo[nBuckets];
                        for (var i = start; i < end; ++i)
                        {
                            var b = (int) (nBuckets * centroidBounds.Offset(primitiveInfo[i].Centroid)[dim]);
                            if (b == nBuckets)
                            {
                                b = nBuckets - 1;
                            }

                            Debug.Assert(b >= 0);
                            Debug.Assert(b < nBuckets);
                            buckets[b].Count++;
                            buckets[b].Bounds += primitiveInfo[i].Bounds;
                        }

                        var cost = new float[nBuckets - 1];
                        for (var i = 0; i < nBuckets - 1; ++i)
                        {
                            var b0 = Bounds.Empty;
                            var b1 = Bounds.Empty;
                            var count0 = 0;
                            var count1 = 0;
                            for (var j = 0; j <= i; ++j)
                            {
                                b0 += buckets[j].Bounds;
                                count0 += buckets[j].Count;
                            }

                            for (var j = i + 1; j < nBuckets; ++j)
                            {
                                b1 += buckets[j].Bounds;
                                count1 += buckets[j].Count;
                            }

                            cost[i] = 1f + (count0 * b0.SurfaceArea() + count1 * b1.SurfaceArea()) /
                                      bounds.SurfaceArea();
                        }

                        var minCost = cost[0];
                        var minCostSplitBucket = 0;
                        for (var i = 1; i < nBuckets - 1; ++i)
                        {
                            if (cost[i] < minCost)
                            {
                                minCost = cost[i];
                                minCostSplitBucket = i;
                            }
                        }

                        var leafCost = nPrimitives;
                        if (nPrimitives > MaxPerNode || minCost < leafCost)
                        {
                            mid = primitiveInfo.Partition(start, end, info =>
                            {
                                var b = (int) (nBuckets * centroidBounds.Offset(info.Centroid)[dim]);
                                if (b == nBuckets)
                                {
                                    b = nBuckets - 1;
                                }

                                Debug.Assert(b >= 0);
                                Debug.Assert(b < nBuckets);
                                return b <= minCostSplitBucket;
                            });
                        }
                        else
                        {
                            var fistPrimOffset = ordered.Count;
                            for (var i = start; i < end; ++i)
                            {
                                var primNum = primitiveInfo[i].PrimitiveNumber;
                                ordered.Add(_p[primNum]);
                            }

                            node.InitLeaf(fistPrimOffset, nPrimitives, bounds);
                            return node;
                        }
                    }

                    break;
            }

            node.InitInterior(dim,
                              RecursiveBuild(arena, primitiveInfo, start, mid, ref totalNodes, ordered),
                              RecursiveBuild(arena, primitiveInfo, mid, end, ref totalNodes, ordered));

            return node;
        }
    }

    public enum SplitMethod
    {
        SAH,
        HLBVH,
        Middle,
        EqualCounts
    }

    internal struct BVHPrimitiveInfo
    {
        public int PrimitiveNumber { get; }
        public Bounds Bounds { get; }

        public BVHPrimitiveInfo(int primitiveNumber, Bounds bounds)
        {
            PrimitiveNumber = primitiveNumber;
            Bounds = bounds;
            Centroid = bounds.Centroid;
        }

        public Point Centroid { get; }
    }

    internal struct BVHBuildNode
    {
        public void InitLeaf(int first, int n, Bounds b)
        {
            FirstPrimitiveOffset = first;
            NPrimitives = n;
            Bounds = b;
            Children = new BVHBuildNode[2];
        }

        public void InitInterior(int axis, BVHBuildNode c0, BVHBuildNode c1)
        {
            Children = new[] {c0, c1};
            Bounds = c0.Bounds + c1.Bounds;
            SplitAxis = axis;
            NPrimitives = 0;
        }

        public int SplitAxis { get; private set; }

        public int NPrimitives { get; private set; }

        public int FirstPrimitiveOffset { get; private set; }

        public Bounds Bounds { get; private set; }

        public BVHBuildNode[] Children { get; private set; }
    }

    internal struct MortonPrimitive
    {
        public int PrimitiveIndex { get; set; }
        public uint MortonCode { get; set; }
    }

    internal class LBVHTreelet
    {
        public int StartIndex { get; set; }
        public int NPrimitives { get; set; }
        public BVHBuildNode[] BuildNodes { get; set; }
    }

    internal struct LinearBVHNode
    {
        // TODO: Combine in union-like struct?
        public int PrimitivesOffset { get; set; }
        public int SecondChildOffset { get; set; }
        public Bounds Bounds { get; set; }
        public ushort NPrimitives { get; set; }
        public byte Axis { get; set; }
        public byte Pad { get; set; }
    }

    internal struct BucketInfo
    {
        public int Count { get; set; }
        public Bounds Bounds { get; set; }
    }
}