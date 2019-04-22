using System.Diagnostics.Contracts;
using static System.MathF;
using static Octans.MathF;

namespace Octans.Reflection.Microfacet
{
    public static class Sampling
    {
        [Pure]
        public static Vector BeckmannSample(Vector wi, in float alphaX, in float alphaY, float u1, float u2)
        {
            var wiStretched = new Vector(alphaX * wi.X, alphaY * wi.Y, wi.Z).Normalize();

            BeckmannSample11(CosTheta(wiStretched), u1, u2, out var slopeX, out var slopeY);

            var tmp = CosPhi(wiStretched) * slopeX - SinPhi(wiStretched) * slopeY;
            slopeY = SinPhi(wiStretched) * slopeX + CosPhi(wiStretched) * slopeY;
            slopeX = tmp;

            slopeX = alphaX * slopeX;
            slopeY = alphaY * slopeY;

            return new Vector(-slopeX, -slopeY, 1f).Normalize();
        }

        private static void BeckmannSample11(float cosThetaI,
                                             in float u1,
                                             in float u2,
                                             out float slopeX,
                                             out float slopeY)
        {
            if (cosThetaI > 0.9999f)
            {
                var r = Sqrt(-Log(1f - u1));
                var sinPhi = Sin(2f * PI * u2);
                var cosPhi = Cos(2f * PI * u2);
                slopeX = r * cosPhi;
                slopeY = r * sinPhi;
                return;
            }

            var sinThetaI = Sqrt(Max(0f, 1f - cosThetaI * cosThetaI));
            var tanThetaI = sinThetaI / cosThetaI;
            var cotThetaI = 1f / tanThetaI;

            var a = -1f;
            var c = Erf(cosThetaI);
            var sampleX = Max(u1, 1e-6f);

            var thetaI = Acos(cosThetaI);
            var fit = 1f + thetaI * (-0.876f + thetaI * (0.4265f - 0.0594f * thetaI));
            var b = c - (1f + c) * Pow(1f - sampleX, fit);

            var normalization = 1f / (1f + c + SqrtPiInv * tanThetaI * Exp(-cotThetaI * cotThetaI));

            var it = 0;
            while (++it < 10)
            {
                if (!(b >= a && b <= c))
                {
                    b = 0.5f * (a + c);
                }

                var invErf = ErfInv(b);
                var value = normalization * (1f + b + SqrtPiInv * tanThetaI * Exp(-invErf * invErf)) - sampleX;


                if (Abs(value) < 1e-5f)
                {
                    break;
                }

                var derivative = normalization * (1f - invErf * tanThetaI);

                if (value > 0f)
                {
                    c = b;
                }
                else
                {
                    a = b;
                }

                b -= value / derivative;
            }

            slopeX = ErfInv(b);
            slopeY = ErfInv(2f * Max(u2, 1e-6f) - 1f);
        }

        public static Vector TrowbridgeReitzSample(Vector wi, in float alphaX, in float alphaY, float u1, float u2)
        {
            var wiStretched = new Vector(alphaX * wi.X, alphaY * wi.Y, wi.Z).Normalize();

            TrowbridgeReitzSample11(CosTheta(wiStretched), u1, u2, out var slopeX, out var slopeY);

            var tmp = CosPhi(wiStretched) * slopeX - SinPhi(wiStretched) * slopeY;
            slopeY = SinPhi(wiStretched) * slopeX + CosPhi(wiStretched) * slopeY;
            slopeX = tmp;

            slopeX = alphaX * slopeX;
            slopeY = alphaY * slopeY;

            return new Vector(-slopeX, -slopeY, 1f).Normalize();
        }

        private static void TrowbridgeReitzSample11(float cosTheta,
                                                    in float u1,
                                                    in float u2,
                                                    out float slopeX,
                                                    out float slopeY)
        {
            if (cosTheta > 0.9999f)
            {
                var r = Sqrt(u1 / (1f - u1));
                var phi = PI * 2f * u2;
                slopeX = r * Cos(phi);
                slopeY = r * Sin(phi);
                return;
            }

            var sinTheta = Sqrt(Max(0f, 1f - cosTheta * cosTheta));
            var tanTheta = sinTheta / cosTheta;

            var a = 1f / tanTheta;
            var G1 = 2f / (1f + Sqrt(1f + 1f / (a * a)));

            var A = 2f * u1 / G1 - 1f;
            var tmp = 1f / (A * A - 1f);
            if (tmp > 1e10f)
            {
                tmp = 1e10f;
            }

            var B = tanTheta;
            var D = Sqrt(Max(B * B * tmp * tmp - (A * A - B * B) * tmp, 0f));
            var slopeX1 = B * tmp - D;
            var slopeX2 = B * tmp + D;
            slopeX = A < 0f || slopeX2 > 1f / tanTheta ? slopeX1 : slopeX2;

            float S;
            float u2s;
            if (u2 > 0.5f)
            {
                S = 1f;
                u2s = 2f * (u2 - 0.5f);
            }
            else
            {
                S = -1f;
                u2s = 2f * (0.5f - u2);
            }

            var z =
                u2s * (u2s * (u2s * 0.27385f - 0.73369f) + 0.46341f) /
                (u2s * (u2s * (u2s * 0.093073f + 0.309420f) - 1.000000f) + 0.597999f);
            slopeY = S * z * Sqrt(1f + slopeX * slopeX);
        }
    }
}