namespace Octans
{
    public class CubeMap : PatternBase
    {
        public CubeMap(ITextureSource left,
                       ITextureSource front,
                       ITextureSource right,
                       ITextureSource back,
                       ITextureSource top,
                       ITextureSource bottom)
        {
            Left = left;
            Front = front;
            Right = right;
            Back = back;
            Top = top;
            Bottom = bottom;
        }

        public ITextureSource Left { get; }
        public ITextureSource Front { get; }
        public ITextureSource Right { get; }
        public ITextureSource Back { get; }
        public ITextureSource Top { get; }
        public ITextureSource Bottom { get; }

        public override Color LocalColorAt(Point localPoint)
        {
            var face = UVMapping.PointToCubeFace(in localPoint);
            float u, v;
            switch (face)
            {
                case UVMapping.CubeFace.Front:
                    (u, v) = UVMapping.CubeUVFrontFace(in localPoint);
                    return Front.PatternAt(u, v);
                case UVMapping.CubeFace.Left:
                    (u, v) = UVMapping.CubeUVLeftFace(in localPoint);
                    return Left.PatternAt(u, v);
                case UVMapping.CubeFace.Right:
                    (u, v) = UVMapping.CubeUVRightFace(in localPoint);
                    return Right.PatternAt(u, v);
                case UVMapping.CubeFace.Top:
                    (u, v) = UVMapping.CubeUVTopFace(in localPoint);
                    return Top.PatternAt(u, v);
                case UVMapping.CubeFace.Bottom:
                    (u, v) = UVMapping.CubeUVBottomFace(in localPoint);
                    return Bottom.PatternAt(u, v);
                case UVMapping.CubeFace.Back:
                    (u, v) = UVMapping.CubeUVBackFace(in localPoint);
                    return Back.PatternAt(u, v);
                default:
                    return Colors.Black;
            }
        }
    }
}