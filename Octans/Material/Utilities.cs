using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Material
{
    public static class Utilities
    {
        public static void Bump(this ITexture2<float> d, SurfaceInteraction si)
        {
            var displace = d.Evaluate(si);

            var du = 0.5f * Abs(si.Dudx) + Abs(si.Dudy);

            // Create small du if no differential available.
            if (du == 0f)
            {
                du = 0.0005f;
            }

            si.P += du * si.ShadingGeometry.Dpdu;
            si.UV += new Vector2(du, 0f);
            si.N = ((Normal) Vector.Cross(si.ShadingGeometry.Dpdu, si.ShadingGeometry.Dpdv) + du * si.Dndu).Normalize();
            var uDisplace = d.Evaluate(si);

            var dv = 0.5f * Abs(si.Dvdx) + Abs(si.Dvdy);

            // Create small dv if no differential available.
            if (dv == 0f)
            {
                dv = 0.0005f;
            }

            si.P += dv * si.ShadingGeometry.Dpdv;
            si.UV += new Vector2(0f, dv);
            si.N = ((Normal) Vector.Cross(si.ShadingGeometry.Dpdu, si.ShadingGeometry.Dpdv) + dv * si.Dndv).Normalize();
            var vDisplace = d.Evaluate(si);

            var dpdu = si.ShadingGeometry.Dpdu +
                       (uDisplace - displace) / du * (Vector) si.ShadingGeometry.N +
                       displace * (Vector) si.ShadingGeometry.Dndu;

            var dpdv = si.ShadingGeometry.Dpdv +
                       (vDisplace - displace) / dv * (Vector) si.ShadingGeometry.N +
                       displace * (Vector) si.ShadingGeometry.Dndv;

            si.SetShadingGeometry(dpdu, dpdv, si.ShadingGeometry.Dndu, si.ShadingGeometry.Dndv, false);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqr(float x) => x * x;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SchlickWeight(float cosTheta)
        {
            var m = Clamp(0, 1, 1 - cosTheta);
            return m * m * (m * m) * m;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FrSchlick(float R0, float cosTheta) => Lerp(R0, 1f, SchlickWeight(cosTheta));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Spectrum FrSchlick(in Spectrum R0, float cosTheta) =>
            Spectrum.Lerp(R0, Spectrum.One, SchlickWeight(cosTheta));


        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SchlickR0FromEta(float eta) => Sqr(eta - 1f) / Sqr(eta + 1f);
    }
}