using System;
using static System.MathF;
using static System.Single;
using static Octans.MathF;

namespace Octans.Reflection.Microfacet
{
    public sealed class BeckmannDistribution : MicrofacetDistributionBase
    {
        private float _alphaX;
        private float _alphaY;

        public BeckmannDistribution(bool sampleVisibleArea = true)
            : base(sampleVisibleArea)
        {
        }

        public BeckmannDistribution()
            : base(true)
        {
        }

        public BeckmannDistribution Initialize(float alphaX, float alphaY, bool sampleVisibleArea = true)
        {
            _alphaX = alphaX;
            _alphaY = alphaY;
            SampleVisibleArea = sampleVisibleArea;
            return this;
        }

        public override float D(in Vector wh)
        {
            var tan2Theta = Tan2Theta(in wh);
            if (IsInfinity(tan2Theta))
            {
                return 0f;
            }

            var cos4Theta = Cos2Theta(in wh) * Cos2Theta(in wh);
            return Exp(-tan2Theta * (Cos2Phi(in wh) / (_alphaX * _alphaX) +
                                     Sin2Phi(in wh) / (_alphaY * _alphaY))) /
                   (PI * _alphaX * _alphaY * cos4Theta);
        }

        public override float Lambda(in Vector w)
        {
            var absTanTheta = Abs(TanTheta(in w));
            if (IsInfinity(absTanTheta))
            {
                return 0f;
            }

            var alpha = Sqrt(Cos2Phi(in w) * _alphaX * _alphaX + Sin2Phi(in w) * _alphaY * _alphaY);
            var a = 1f / (alpha * absTanTheta);
            if (a >= 1.6f)
            {
                return 0f;
            }

            return (1f - 1.259f * a + 0.0396f * a * a) / (3.535f * a + 2.181f * a * a);
        }

        public override Vector SampleWh(in Vector wo, in Point2D u)
        {
            if (!SampleVisibleArea)
            {
                float tan2Theta;
                float phi;
                if (_alphaX == _alphaY)
                {
                    var logSample = Log(1f - u[0]);
                    tan2Theta = -_alphaX * _alphaY * logSample;
                    phi = u[1] * 2f * PI;
                }
                else
                {
                    var logSample = Log(1f - u[0]);
                    phi = Atan(_alphaY / _alphaX * Tan(2f * PI * u[1] + 0.5f * PI));
                    if (u[1] > 0.5f)
                    {
                        phi += PI;
                    }

                    var sinPhi = Sin(phi);
                    var cosPhi = Cos(phi);
                    var alphaX2 = _alphaX * _alphaX;
                    var alphaY2 = _alphaY * _alphaY;
                    tan2Theta = -logSample / (cosPhi * cosPhi / alphaX2 + sinPhi * sinPhi / alphaY2);
                }

                var cosTheta = 1f / Sqrt(1f + tan2Theta);
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
                var wh = Sampling.BeckmannSample(flip ? -wo : wo, _alphaX, _alphaY, u[0], u[1]);
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
    }
}