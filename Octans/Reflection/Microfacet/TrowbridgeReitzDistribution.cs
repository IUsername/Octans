using System.Diagnostics.Contracts;
using static System.MathF;
using static System.Single;
using static Octans.MathF;

namespace Octans.Reflection.Microfacet
{
    public class TrowbridgeReitzDistribution : MicrofacetDistributionBase
    {
        private float _alphaX;
        private float _alphaY;

        public TrowbridgeReitzDistribution(bool sampleVisibleArea = true)
            : base(sampleVisibleArea)
        {
        }

        public TrowbridgeReitzDistribution()
            : base(true)
        {
        }

        public TrowbridgeReitzDistribution Initialize(float alphaX, float alphaY, bool sampleVisibleArea = true)
        {
            _alphaX = alphaX;
            _alphaY = alphaY;
            SampleVisibleArea = sampleVisibleArea;
            return this;
        }

        public override float D(in Vector wh)
        {
            var a2 = _alphaX * _alphaX;
            var e = (Cos2Theta(in wh) * (a2 - 1f) + 1f);
            return a2 / ((PI) * (e * e));
        
            //var tan2Theta = Tan2Theta(in wh);
            //if (IsInfinity(tan2Theta))
            //{
            //    return 0f;
            //}

            //var cos4Theta = Cos2Theta(in wh) * Cos2Theta(in wh);
            //var e = (Cos2Phi(in wh) / (_alphaX * _alphaX) + Sin2Phi(in wh) / (_alphaY * _alphaY)) * tan2Theta;
            //return 1f / (PI * _alphaX * _alphaY * cos4Theta * (1f + e) * (1f + e));
        }

        public override float Lambda(in Vector w)
        {
            var absTanTheta = Abs(TanTheta(in w));
            if (IsInfinity(absTanTheta))
            {
                return 0f;
            }

            var alpha = Sqrt(Cos2Phi(in w) * _alphaX * _alphaX + Sin2Phi(in w) * _alphaY * _alphaY);
            var alpha2Tan2Theta = (alpha * absTanTheta) * (alpha * absTanTheta);
            return (-1f + Sqrt(1f + alpha2Tan2Theta)) / 2f;
        }

        public override Vector SampleWh(in Vector wo, in Point2D u)
        {
            if (!SampleVisibleArea)
            {
                float cosTheta;
                var phi = 2f * PI * u[1];
                if (_alphaX == _alphaY)
                {
                    var tanTheta2 = _alphaX * _alphaX * u[0] / (1f - u[0]);
                    cosTheta = 1f / Sqrt(1f + tanTheta2);
                }
                else
                {
                    phi = Atan(_alphaY / _alphaX * Tan(2f * PI * u[1] + 0.5f * PI));
                    if (u[1] > 0.5f)
                    {
                        phi += PI;
                    }

                    var sinPhi = Sin(phi);
                    var cosPhi = Cos(phi);
                    var alphaX2 = _alphaX * _alphaX;
                    var alphaY2 = _alphaY * _alphaY;
                    var alpha2 = 1f / (cosPhi * cosPhi / alphaX2 + sinPhi * sinPhi / alphaY2);
                    var tanTheta2 = alpha2 * u[0] / (1f - u[0]);
                    cosTheta = 1f / Sqrt(1f + tanTheta2);
                }

                var sinTheta = Sqrt(Max(0f, 1f - cosTheta * cosTheta));
                var wh = SphericalDirection(sinTheta, cosTheta, phi);
                if (!IsInSameHemisphere(wo, wh))
                {
                    wh = -wh;
                }

                return wh;
            }
            else
            {
                var flip = wo.Z < 0f;
                var wh = Sampling.TrowbridgeReitzSample(flip ? -wo : wo, _alphaX, _alphaY, u[0], u[1]);
                if (flip)
                {
                    wh = -wh;
                }

                return wh;
            }
        }

        public override float Pdf(in Vector wo, in Vector wh)
        {
            if (SampleVisibleArea)
            {
                return D(in wh) * G1(in wo) * AbsDot(wo, wh) / AbsCosTheta(wo);
            }

            return D(wh) * AbsCosTheta(wh);
        }

        [Pure]
        public static float RoughnessToAlpha(in float roughness) => roughness.RoughnessToAlpha();
    }
}