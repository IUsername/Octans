using System;
namespace Octans.Shading
{
    public class GGXNormalDistribution : INormalDistribution
    {
        public static INormalDistribution Instance = new GGXNormalDistribution();

        private GGXNormalDistribution()
        {
        }

        public float Factor(in ShadingInfo info) => GGXNormalDist(info.Alpha, info.Roughness, info.NdotH);

        private static float GGXNormalDist(float alpha, float roughness, float NdotH)
        {
            const float oneOverPi = 1f / MathF.PI;
            var NdotHSqr = NdotH * NdotH;
            var tanNdotHSqr = (1 - NdotHSqr) / NdotHSqr;
            var s = roughness / (NdotHSqr * (alpha + tanNdotHSqr));
            return oneOverPi * MathF.Sqrt(s);
        }

        public (Vector wi, Color reflectance) Sample(in ShadingInfo info, in LocalFrame localFrame, float e0, float e1)
        {
          //  var wo = localFrame.ToLocal(info.ViewReflectDirection);
            var wo = localFrame.ToLocal(info.ViewDirection);

           // var wo = info.ViewReflectDirection;
            var wm = GGXVndf(wo, info.Alpha, e0, e1);
            var wi = -wo.Reflect(wm);
       //       var wi = 2f * (wm % wo) * wm - wo;
        //    var wi = wo.Reflect(wm);
       //     wi = new Vector(wi.X, -wi.Y, wi.Z);




            // The following is always going to check is wi.Z > 0 since local frame Z is (0,0,+1)
            var n = localFrame.ToLocal(info.NormalDirection);
            var NdotWi = MathFunction.Saturate(n % wi);
            //if (NdotWi > 0)
            if (NdotWi > 0)
            {

                var F =  SchlickFresnel(info.SpecularColor, wi%wm);
                var G1 = SmithGGXMasking(in n, in wi,  in wo, info.Alpha);
                var G2 = SmithGGXMaskingShadowing(in n, in wi, in wo, info.Alpha);
                var reflectance =  F * (G2 / G1);
                return (wi, reflectance);
            }

            return (wi, Colors.Black);

        }

        private float SmithGGXMaskingShadowing(in Vector normal, in Vector wi, in Vector wo, float alpha)
        {
            var NdotL = MathFunction.Saturate(normal % wi);
            var NdotV = MathFunction.Saturate(normal % wo);

            var denomA = NdotV * MathF.Sqrt(alpha + (1f - alpha) * NdotL * NdotL);
            var denomB = NdotL * MathF.Sqrt(alpha + (1f - alpha) * NdotV * NdotV);
            return 2f * NdotL * NdotV / (denomA + denomB);
        }

        private float SmithGGXMasking(in Vector normal, in Vector wi, in Vector wo, float alpha)
        {
            var NdotL =MathFunction.Saturate(normal % wi);
            var NdotV = MathFunction.Saturate(normal % wo);
            var denomC = MathF.Sqrt(alpha + (1f - alpha) * NdotV * NdotV) + NdotV;
            return 2f * NdotV / denomC;
        }

        public static Vector GGXVndf(Vector ve, float roughness, float e0, float e1)
        {
            // Stretch to sample with roughness of 1
            var v = new Vector(roughness * ve.X, roughness * ve.Y, ve.Z).Normalize();
            var T1 = (v.Z < 0.9999f) ? Vector.Cross(new Vector(0, 0, 1),v).Normalize() : new Vector(1, 0, 0);
            var T2 = Vector.Cross(v,T1);

            // Project proportionally onto each half of disk
            var r = MathF.Sqrt(e0);
            var phi = 2f * MathF.PI * e1;
            var t1 = r * MathF.Cos(phi);
            var t2 = r * MathF.Sin(phi);
            var s = 0.5f * (1f + v.Z);
            t2 = (1f - s) * MathF.Sqrt(1f - t1 * t1) + s * t2;
            var Nh = t1 * T1 + t2 * T2 + MathF.Sqrt(MathF.Max(0f, 1f - t1 * t1 - t2 * t2)) * v;
            var Ne = new Vector(Nh.X * roughness, Nh.Y * roughness, MathF.Max(0f, Nh.Z)).Normalize();
            return Ne;
        }

        private static Color SchlickFresnel(in Color specularColor, float WIdotWM)
        {
            return specularColor + (Colors.White - specularColor) * MathF.Pow(1f - MathFunction.Saturate(WIdotWM), 5);
        }


        private static float SchlickFresnel(in ShadingInfo si, float WIdotWM)
        {
            return si.F0 + (1f - si.F0) * MathF.Pow(1f - WIdotWM, 5);
        }

        private static float SchlickFresnelFunc(float i)
        {
            var x = MathFunction.Saturate(1f - i);
            var x2 = x * x;
            return x2 * x2 * x;
        }
    }
}