namespace Octans
{
    public static class TransformExtensions
    {
        public static Transform Translate(this Transform m, float x, float y, float z) =>
            Transform.Translate(x, y, z) * m;

        public static Transform Apply(this Transform m, in Transform n) => n * m;

        public static Transform TranslateX(this Transform m, float x) => Transform.TranslateX(x) * m;

        public static Transform TranslateY(this Transform m, float y) => Transform.TranslateY(y) * m;

        public static Transform TranslateZ(this Transform m, float z) => Transform.TranslateX(z) * m;

        public static Transform Scale(this Transform m, float x, float y, float z) => Transform.Scale(x, y, z) * m;

        public static Transform Scale(this Transform m, float factor) => Transform.Scale(factor) * m;

        public static Transform RotateX(this Transform m, float rad) => Transform.RotateX(rad) * m;

        public static Transform RotateY(this Transform m, float rad) => Transform.RotateY(rad) * m;

        public static Transform RotateZ(this Transform m, float rad) => Transform.RotateZ(rad) * m;

        public static Transform Shear(this Transform m, float xy, float xz, float yx, float yz, float zx, float zy) =>
            Transform.Shear(xy, xz, yx, yz, zx, zy) * m;
    }
}