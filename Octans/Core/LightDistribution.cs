using System;
using System.Diagnostics;
using System.Linq;
using Octans.Integrator;
using Octans.Sampling;
using static System.Math;
using static Octans.Sampling.QuasiRandom;

namespace Octans
{
    public enum LightSampleStrategy
    {
        Uniform,
        Power,
        Spatial
    }

    public abstract class LightDistribution
    {
        protected LightDistribution(IScene scene)
        {
            Scene = scene;
        }

        protected IScene Scene { get; }

        public abstract Distribution1D Lookup(in Point p);

        public static LightDistribution CreateLightSampleDistribution(LightSampleStrategy strategy, in IScene scene)
        {
            if (scene.Lights.Length == 1)
            {
                strategy = LightSampleStrategy.Uniform;
            }

            switch (strategy)
            {
                case LightSampleStrategy.Uniform:
                    return new UniformLightDistribution(scene);
                case LightSampleStrategy.Power:
                    return new PowerLightDistribution(scene);
                case LightSampleStrategy.Spatial:
                    return new SpatialLightDistribution(scene);
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }
        }
    }

    public sealed class SpatialLightDistribution : LightDistribution
    {
        private const ulong InvalidPackedPos = 0xffffffffffffffff;
        private readonly HashEntry[] _hashTable;
        private readonly int[] _nVoxels;
        private readonly object _padlock = new object();
        private IScene _scene;

        public SpatialLightDistribution(in IScene scene, int maxVoxels = 64) : base(scene)
        {
            var b = scene.WorldBounds;
            var diag = b.Diagonal();
            var bMax = diag[b.MaximumExtent()];
            _nVoxels = new int[3];
            for (var i = 0; i < 3; ++i)
            {
                _nVoxels[i] = Max(1, (int) Round(diag[i] / bMax * maxVoxels));
                Debug.Assert(_nVoxels[i] <= 1 << 20);
            }

            var hashTableSize = 4 * _nVoxels[0] * _nVoxels[1] * _nVoxels[2];
            _hashTable = new HashEntry[hashTableSize];
            for (var i = 0; i < hashTableSize; ++i)
            {
                _hashTable[i] = new HashEntry
                {
                    PackedPos = InvalidPackedPos,
                    Distribution = null
                };
            }

            _scene = scene;
        }

        public override Distribution1D Lookup(in Point p)
        {
            var offset = Scene.WorldBounds.Offset(p);
            var pi = new Point3I();
            for (var i = 0; i < 3; ++i)
            {
                pi[i] = Math.Clamp(0, _nVoxels[i] - 1, (int) (offset[i] * _nVoxels[i]));
            }

            var size = (ulong) _hashTable.Length;

            var packedPos = ((ulong) pi[0] << 40) | ((ulong) pi[1] << 20) | (ulong) pi[2];
            Debug.Assert(packedPos != InvalidPackedPos);

            var hash = packedPos;
            hash ^= hash >> 31;
            hash *= 0x7fb5d329728ea185;
            hash ^= hash >> 27;
            hash *= 0x81dadef4bc2dd44d;
            hash ^= hash >> 33;
            hash %= size;

            var step = 1;
            while (true)
            {
                ref var entry = ref _hashTable[hash];
                if (entry.PackedPos == packedPos)
                {
                    return entry.Distribution;
                }

                if (entry.PackedPos != InvalidPackedPos)
                {
                    hash += (ulong) (step * step);
                    if (hash > size)
                    {
                        hash %= size;
                    }

                    ++step;
                }
                else
                {
                    lock (_padlock)
                    {
                        entry = ref _hashTable[hash];
                        if (entry.PackedPos != InvalidPackedPos)
                        {
                            continue;
                        }

                        var dist = ComputeDistribution(pi);
                        entry.Distribution = dist;
                        entry.PackedPos = packedPos;
                        return dist;
                    }
                }
            }
        }

        private Distribution1D ComputeDistribution(Point3I pi)
        {
            var p0 = new Point(
                (float) pi[0] / _nVoxels[0],
                (float) pi[1] / _nVoxels[1],
                (float) pi[2] / _nVoxels[2]);

            var p1 = new Point(
                (float) (pi[0] + 1) / _nVoxels[0],
                (float) (pi[1] + 1) / _nVoxels[1],
                (float) (pi[2] + 1) / _nVoxels[2]);

            var voxelBounds = new Bounds(
                Scene.WorldBounds.Lerp(p0),
                Scene.WorldBounds.Lerp(p1));

            const int nSamples = 128;
            var lightContrib = new float[Scene.Lights.Length];
            for (var i = 0; i < nSamples; ++i)
            {
                var rp = new Point(
                    RadicalInverse(0, (ulong) i),
                    RadicalInverse(1, (ulong)i),
                    RadicalInverse(2, (ulong)i));
                var po = voxelBounds.Lerp(rp);

                var intr = new Interaction();
                intr.Initialize(po, Normals.Zero, Vectors.Zero, Vectors.XAxis);

                var u = new Point2D(RadicalInverse(i, 3), RadicalInverse(i, 4));
                for (var j = 0; j < Scene.Lights.Length; ++j)
                {
                    var Li = Scene.Lights[j].Sample_Li(intr, u, out _, out var pdf, out var vis);
                    if (pdf > 0f)
                    {
                        var factor = vis.Unoccluded(_scene) ? 1f : 0.1f;
                        lightContrib[j] += factor * Li.YComponent() / pdf;

                        //lightContrib[j] +=  Li.YComponent() / pdf;
                    }
                }
            }

            var sumContrib = lightContrib.Sum();
            var avgContrib = sumContrib / (nSamples * lightContrib.Length);
            var minContrib = avgContrib > 0f ? 0.001f * avgContrib : 1f;
            for (var i = 0; i < lightContrib.Length; ++i)
            {
                lightContrib[i] = System.MathF.Max(lightContrib[i], minContrib);
            }

            return new Distribution1D(lightContrib, lightContrib.Length);
        }

        private class HashEntry
        {
            public ulong PackedPos { get; set; }
            public Distribution1D Distribution { get; set; }
        }
    }

    public sealed class PowerLightDistribution : LightDistribution
    {
        private readonly Distribution1D _distribution;

        public PowerLightDistribution(in IScene scene) : base(scene)
        {
            _distribution = scene.ComputeLightPowerDistribution();
        }

        public override Distribution1D Lookup(in Point p) => _distribution;
    }

    public sealed class UniformLightDistribution : LightDistribution
    {
        private readonly Distribution1D _distribution;

        public UniformLightDistribution(in IScene scene) : base(scene)
        {
            var prob = new float[scene.Lights.Length];
            Array.Fill(prob, 1f);
            _distribution = new Distribution1D(prob, prob.Length);
        }

        public override Distribution1D Lookup(in Point p) => _distribution;
    }
}