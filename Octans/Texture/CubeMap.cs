namespace Octans.Texture
{
    public class CubeMap : TextureBase
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

        public override Color LocalColorAt(in Point localPoint)
        {
            var face = UVMapping.PointToCubeFace(in localPoint);
            UVPoint uv;
            switch (face)
            {
                case UVMapping.CubeFace.Front:
                    uv = UVMapping.CubeUVFrontFace(in localPoint);
                    return Front.ColorAt(uv);
                case UVMapping.CubeFace.Left:
                    uv = UVMapping.CubeUVLeftFace(in localPoint);
                    return Left.ColorAt(uv);
                case UVMapping.CubeFace.Right:
                    uv = UVMapping.CubeUVRightFace(in localPoint);
                    return Right.ColorAt(uv);
                case UVMapping.CubeFace.Top:
                    uv = UVMapping.CubeUVTopFace(in localPoint);
                    return Top.ColorAt(uv);
                case UVMapping.CubeFace.Bottom:
                    uv = UVMapping.CubeUVBottomFace(in localPoint);
                    return Bottom.ColorAt(uv);
                case UVMapping.CubeFace.Back:
                    uv = UVMapping.CubeUVBackFace(in localPoint);
                    return Back.ColorAt(uv);
                default:
                    return Colors.Black;
            }
        }
    }
}