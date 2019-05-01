using System;
using System.Collections.Generic;

namespace Octans.Primitive
{
    public class TriangleMesh
    {
        //private readonly int _nTriangles;
        //private readonly int _nVertices;
        private readonly int[] _vertexIndices;

        public TriangleMesh(Transform objectToWorld,
                            int nTriangles,
                            int[] vertexIndices,
                            int nVertices,
                            in Point[] p,
                            in Vector[] s,
                            in Normal[] n,
                            in Point2D[] uv,
                            ITexture2<float> alphaMask,
                            ITexture2<float> shadowAlphaMask,
                            int[] faceIndices)
        {
            //_nTriangles = nTriangles;
            _vertexIndices = new int[nTriangles * 3];
            Array.Copy(vertexIndices, _vertexIndices, nTriangles * 3);
            //_nVertices = nVertices;
            AlphaMask = alphaMask;
            HasAlphaMask = !(alphaMask is null);
            ShadowAlphaMask = shadowAlphaMask;
            HasShadowAlphaMask = !(shadowAlphaMask is null);

            P = new Point[nVertices];
            for (var i = 0; i < nVertices; ++i)
            {
                P[i] = objectToWorld * p[i];
            }

            HasUV = !(uv is null) && uv.Length > 0;
            if (HasUV)
            {
                UV = new Point2D[nVertices];
                Array.Copy(uv, UV, nVertices);
            }

            HasN = !(n is null) && n.Length > 0;
            if (HasN)
            {
                N = new Normal[nVertices];
                for (var i = 0; i < nVertices; ++i)
                {
                    N[i] = objectToWorld * n[i];
                }
            }

            HasS = !(s is null) && s.Length > 0;
            if (HasS)
            {
                S = new Vector[nVertices];
                for (var i = 0; i < nVertices; ++i)
                {
                    S[i] = objectToWorld * s[i];
                }
            }

            HasFaceIndices = !(faceIndices is null) && faceIndices.Length > 0f;
            if (HasFaceIndices)
            {
                FaceIndices = new int[nTriangles];
                Array.Copy(faceIndices, FaceIndices, nTriangles);
            }
        }

        public int[] FaceIndices { get; }
        public Point2D[] UV { get; }
        public Point[] P { get; }
        public Normal[] N { get; }
        public bool HasAlphaMask { get; }
        public bool HasShadowAlphaMask { get; }
        public ITexture2<float> AlphaMask { get; }
        public ITexture2<float> ShadowAlphaMask { get; }
        public bool HasN { get; }
        public bool HasS { get; }
        public bool HasFaceIndices { get; }
        public Vector[] S { get; }
        public bool HasUV { get; }

        public ReadOnlyMemory<int> VertexIndicesSpan(in int triNumber) =>
            new ReadOnlyMemory<int>(_vertexIndices, triNumber * 3, 3);


        public static IEnumerable<IShape> CreateTriangleMesh(Transform objectToWorld,
                                                             Transform worldToObject,
                                                             bool reverseOrientation,
                                                             int nTriangles,
                                                             int[] vertexIndices,
                                                             int nVertices,
                                                             Point[] p,
                                                             Vector[] s,
                                                             Normal[] n,
                                                             Point2D[] uv,
                                                             ITexture2<float> alphaMask,
                                                             ITexture2<float> shadowAlphaMask,
                                                             int[] faceIndices)
        {
            var mesh = new TriangleMesh(objectToWorld, nTriangles, vertexIndices, nVertices, p, s, n, uv, alphaMask,
                                        shadowAlphaMask, faceIndices);
            for (var i = 0; i < nTriangles; ++i)
            {
                yield return new Triangle(objectToWorld, worldToObject, reverseOrientation, mesh, i);
            }
        }
    }
}