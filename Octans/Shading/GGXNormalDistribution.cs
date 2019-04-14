using System;

namespace Octans.Shading
{
    public class GGXNormalDistribution : INormalDistribution
    {
        public static INormalDistribution Instance = new GGXNormalDistribution();

        private GGXNormalDistribution()
        {
        }

        public float Factor(in ShadingInfo info) => GGXNormalDist2(info.Alpha, info.NdotH);

        private static float GGXNormalDist(float alpha, float roughness, float NdotH)
        {
            const float oneOverPi = 1f / MathF.PI;
            var NdotHSqr = NdotH * NdotH;
            var tanNdotHSqr = (1 - NdotHSqr) / NdotHSqr;
            var s = roughness / (NdotHSqr * (alpha + tanNdotHSqr));
            return oneOverPi * MathF.Sqrt(s);
        }

        private static float GGXNormalDist2(float alpha,float NdotH)
        {
            var denom = NdotH * NdotH * (alpha - 1f) + 1f;
            return alpha / (MathF.PI * denom * denom);
        }

        public (Vector wi, Color reflectance) Sample(in IntersectionInfo info,
                                                     in LocalFrame localFrame,
                                                     float e0,
                                                     float e1)
        {
            var wo = localFrame.ToLocal(info.Eye);
            // TODO: Handle Z == 0 better.
            if (wo.Z <= 0f)
            {
                return (wo, Colors.Black);
            }
            var wm = GGXVndf(wo, info.Roughness, info.Roughness, e0, e1);
            var wi = -wo.Reflect(wm);

            // The following is always going to check is wi.Z > 0 since local frame Z is (0,0,+1)
            //var n = localFrame.ToLocal(info.Normal);
            //var NdotWi = MathFunction.Saturate(n % wi);
            //if (NdotWi > 0)
            if (!(wi.Z > 0))
            {
                // Below surface.
                return (wi, Colors.Black);
            }


            // TODO: This is repeated in ShadingInfo.
            var color =
                info.Geometry.Material.Texture.ShapeColor(info.Geometry, info.OverPoint);
            var specularColor = Color.Lerp(info.Geometry.Material.SpecularColor, color, info.Geometry.Material.Metallic * 0.5f);


            var F =  SchlickFresnel(specularColor, wi%wm);
            //var G1 = SmithGGXMasking(in wm, in wi, in wo, info.Alpha);
            //var G2 = SmithGGXMaskingShadowing(in wm, in wi, in wo, info.Alpha);
            var G1 = G1SmithApprox(in wm, in wo, info.Alpha);
            var G2 = G2SmithApprox(in wm, in wi, in wo, info.Alpha);
            var reflectance =  F * (G2 / G1);
            return (wi, reflectance);
        }

        public (Vector wi, Color transmissionFactor) SampleTransmission(in IntersectionInfo info,
                                                                        in LocalFrame localFrame,
                                                                        float e0,
                                                                        float e1)
        {
            var wo = localFrame.ToLocal(info.Eye);
            // TODO: Handle Z == 0 better.
            if (wo.Z <= 0f)
            {
                return (wo, Colors.Black);
            }
            var wm = GGXVndf(wo, info.Roughness, info.Roughness, e0, e1);

            var nRatio = info.N1 / info.N2;
            var cosI = wo % wm;
            var sin2T = nRatio * nRatio * (1f - cosI * cosI);
            if (sin2T > 1f)
            {
                // Total internal reflection.
                return (new Vector(), Colors.Black);
            }

            var cosT = MathF.Sqrt(1f - sin2T);
            var wi = wm * (nRatio * cosI - cosT) - wo * nRatio;


            //var wi = -wo.Reflect(wm);

            // The following is always going to check is wi.Z > 0 since local frame Z is (0,0,+1)
            //var n = localFrame.ToLocal(info.Normal);
            //var NdotWi = MathFunction.Saturate(n % wi);
            //if (NdotWi > 0)
            //if (!(wi.Z > 0))
            //{
            //    // Below surface.
            //    return (wi, Colors.Black);
            //}

            //var tF = EvalTransmission(Colors.White, in info, in wo, in wm,
            //                          in wi);
            return (wi, Colors.White);
        }

        //public Color Transmission(in ShadingInfo si, in IntersectionInfo info, in Vector wo, in Vector wm, in Vector wi)
        //{
        //    return EvalTransmission(si.DiffuseColor, in info, in wo, in wm, in wi);
        //}

        private static Color EvalTransmission(Color baseColor, in IntersectionInfo info, in Vector wo, in Vector wm, in Vector wi)
        {
            var relativeIor = info.N1 / info.N2;
            var n2 = relativeIor * relativeIor;

            var h = (wo + wi).Normalize();

            var HdotL = h % wi;
            var HdotV = h % wo;
            var absNdotL = Vector.AbsDot(wm, wi);
            var absNdotV = Vector.AbsDot(wm, wo);
            var absHdotL = MathF.Abs(HdotL);
            var absHdotV = MathF.Abs(HdotV);

            var d = GGXNormalDist2(info.Alpha, wm%h);
            var gl = G1SmithApprox(in wm, in wi, info.Alpha);
            var gv = G1SmithApprox(in wm, in wo, info.Alpha);
            var f = DielectricFresnel(HdotV, 1f, 1f / relativeIor);

            // TODO: Handle thin surfaces

            var c = (absHdotL * absHdotV) / (absNdotL * absNdotV);
            var p = HdotL  + relativeIor * HdotV;
            var t = n2 / (p * p);

            return baseColor * c * t * (1f - f) * gl * gv * d;
        }

        private static float DielectricFresnel(float cosThetaI, float etaI, float etaT)
        {
            cosThetaI = MathFunction.ClampF(-1, 1, cosThetaI);
            bool entering = cosThetaI > 0f;
            if (!entering)
            {
                (etaI, etaT) = (etaT, etaI);
                cosThetaI = MathF.Abs(cosThetaI);
            }

            var sinThetaI = MathF.Sqrt(MathF.Max(0f, 1f - cosThetaI * cosThetaI));
            var sinThetaT = etaI / etaT * sinThetaI;

            if (sinThetaT >= 1) return 1f;

            var cosThetaT = MathF.Sqrt(MathF.Max(0f, 1f - sinThetaT * sinThetaT));
            var Rparl = ((etaT * cosThetaI) - (etaI * cosThetaT)) / ((etaT * cosThetaI) + (etaI * cosThetaT));
            var Rperp = ((etaI * cosThetaI) - (etaT * cosThetaT)) / ((etaI * cosThetaI) + (etaT * cosThetaT));
            return (Rparl * Rparl + Rperp * Rperp) / 2f;

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
            //var NdotL =MathFunction.Saturate(normal % wi);
            var NdotV = MathFunction.Saturate(normal % wo);
            var denomC = MathF.Sqrt(alpha + (1f - alpha) * NdotV * NdotV) + NdotV;
            return 2f * NdotV / denomC;
        }

        private static float G1SmithApprox(in Vector normal, in Vector wo, float alpha)
        {
            var NdotV =normal % wo;
            return (2f * NdotV) / (NdotV * (2f - alpha) + alpha);
        }

        private static float G2SmithApprox(in Vector normal, in Vector wi, in Vector wo, float alpha)
        {
            var NdotL = Vector.AbsDot(normal, wi);
            var NdotV = Vector.AbsDot(normal, wo);

            var a = 2f * NdotL * NdotV;
            var denom = MathFunction.Lerp(a, NdotL + NdotV, alpha);
            return a / denom;

        }

        //private static float GGX(float alpha, float NdotX)
        //{
        //    var t = MathF.Sqrt(alpha + (1f - alpha) * (NdotX * NdotX));
        //    return 2 * NdotX / ((NdotX) + t);
        //}

        public static Vector GGXVndf(Vector ve, float alphaX, float alphaY, float e0, float e1)
        {
            // Stretch to sample with roughness of 1
            var v = new Vector(alphaX * ve.X, alphaY * ve.Y, ve.Z).Normalize();
            var T1 = (v.Z < 0.9999f) ? Vector.Cross(Vectors.ZAxis, v).Normalize() : Vectors.XAxis;
            var T2 = Vector.Cross(v,T1);

            // Project proportionally onto each half of disk
            var r = MathF.Sqrt(e0);
            var phi = 2f * MathF.PI * e1;
            var t1 = r * MathF.Cos(phi);
            var t2 = r * MathF.Sin(phi);
            var s = 0.5f * (1f + v.Z);
            t2 = (1f - s) * MathF.Sqrt(1f - t1 * t1) + s * t2;
            var Nh = t1 * T1 + t2 * T2 + MathF.Sqrt(MathF.Max(0f, 1f - t1 * t1 - t2 * t2)) * v;
            var Ne = new Vector(Nh.X * alphaX, Nh.Y * alphaY, MathF.Max(0f, Nh.Z)).Normalize();
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