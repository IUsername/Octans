using System;
using System.ComponentModel.Design;
using System.Security.Cryptography;

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

        public (Vector wi, Color reflectance) Sample(in ShadingInfo info, float e0, float e1)
        {
            var wo = info.ViewDirection;
            var wm = GGXVndf(wo, info.Alpha, e0, e1);
          //  var wi = 2f * (wo % wm) * wm - wo;
            var wi = -wm.Reflect(info.NormalDirection);
          
           
            //var wi = Vector.Reflect(info.ViewDirection, wm);

            var n = info.NormalDirection;
            var NdotWi = MathFunction.Saturate(n % wi);
            if (NdotWi > 0)
            {

                var F =  SchlickFresnel(info.SpecularColor, wm%wi);
                var G1 = SmithGGXMasking(in n, in wi,  in wo, info.Alpha);
                var G2 = SmithGGXMaskingShadowing(in n, in wi, in wo, info.Alpha);
                var reflectance =  F * (G2 / G1);
                return (wi, reflectance);
            }

            return (wi, Colors.Blue);

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

        //public static Vector GGXVndf(Vector eye, float roughness, float e0, float e1)
        //{
        //    if (roughness < 0.000001f)
        //    {
        //        return eye;
        //    }

        //    // Stretch to sample with roughness of 1
        //    var v = new Vector(roughness * eye.X, eye.Y, roughness * eye.Z).Normalize();

        //    var t1 = (v.Y < 0.9999f) ? Vector.Cross(new Vector(0, 1, 0), v).Normalize() : new Vector(0, 0, 1);
        //    var t2 = Vector.Cross(t1, v);

        //    // Project proportionally onto each half of disk
        //    var a = 1f / (1f + v.Y);
        //    var r = MathF.Sqrt(e0);
        //    var phi = (e1 < a) ? (e1 / a) * MathF.PI : MathF.PI + (e1 - a) / (1f - a) * MathF.PI;
        //    var p1 = r * MathF.Cos(phi);
        //    var p2 = r * MathF.Sin(phi) * ((e1 < a) ? 1f : v.Y);

        //    var n = p1 * t1 + p2 * t2 + MathF.Sqrt(MathF.Max(0f, 1f - p1 * p1 - p2 * p2)) * v;
        //    var scaled = new Vector(n.X * a, MathF.Max(0f, n.Y), n.Z * a);
        //    var norm = scaled.Normalize();
        //    return norm;
        //}

        //public static Vector GGXVndf(Vector eye, float a, float e0, float e1)
        //{
        //    var theta = MathF.Acos(MathF.Sqrt(1f - e0) / ((a - 1f) * e0 + 1f));
        //    var phi = 2f * MathF.PI * e1;
        //    var x = MathF.Sin(theta) * MathF.Cos(phi);
        //    var y = MathF.Cos(theta);
        //    var z = MathF.Sin(theta) * MathF.Sin(phi);
        //    return new Vector(x,y,z).Normalize();
        //}

        //public static Vector GGXVndf(Vector wO, float roughness, float e0, float e1)
        //{
        //    // Stretch to sample with roughness of 1
        //    var v = new Vector(roughness * wO.X, wO.Y, roughness * wO.Z).Normalize();
        //    var T1 = (v.Y < 0.9999f) ? Vector.Cross(v, new Vector(0, 1, 0)).Normalize() : new Vector(1, 0, 0);
        //    var T2 = Vector.Cross(T1, v);

        //    // Project proportionally onto each half of disk
        //    var r = MathF.Sqrt(e0);
        //    var phi = 2f * MathF.PI * e1;
        //    var t1 = r * MathF.Cos(phi);
        //    var t2 = r * MathF.Sin(phi);
        //    var s = 0.5f * (1 + v.Y);
        //    t2 = (1f - s) * MathF.Sqrt(1f - t1 * t1) + s * t2;
        //    var Nh = t1 * T1 + t2 * T2 + MathF.Sqrt(MathF.Max(0f, 1f - t1 * t1 - t2 * t2)) * v;
        //    var Ne = new Vector(Nh.X * roughness, MathF.Max(0f, Nh.Y), Nh.Z * roughness).Normalize();

        //    return Ne;
        //}

        public static Vector GGXVndf(Vector eye, float roughness, float e0, float e1)
        {
            if (roughness < 0.000001f)
            {
                return eye;
            }

            // Stretch to sample with roughness of 1
            var v = new Vector(roughness * eye.X, eye.Y, roughness * eye.Z).Normalize();

            var t1 = (v.Y < 0.9999f) ? Vector.Cross(v, new Vector(0, 1, 0)).Normalize() : new Vector(0, 0, 1);
            var t2 = Vector.Cross(t1, v);

            // Project proportionally onto each half of disk
            var a = 1f / (1f + v.Y);
            var r = MathF.Sqrt(e0);
            var phi = (e1 < a) ? (e1 / a) * MathF.PI : MathF.PI + (e1 - a) / (1f - a) * MathF.PI;
            var p1 = r * MathF.Cos(phi);
            var p2 = r * MathF.Sin(phi) * ((e1 < a) ? 1f : v.Y);

            var n = p1 * t1 + p2 * t2 + MathF.Sqrt(MathF.Max(0f, 1f - p1 * p1 - p2 * p2)) * v;
            var scaled = new Vector(n.X * roughness, MathF.Max(0f, n.Y), n.Z * roughness);
            //var scaled = new Vector(n.X / a, MathF.Max(0f, n.Y), n.Z / a);
            var norm = scaled.Normalize();
            return norm;
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