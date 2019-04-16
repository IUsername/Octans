using System;
using static System.MathF;
using static System.Single;
using static Octans.MathF;

namespace Octans.Reflection.Microfacet
{
    public sealed class TrowbridgeReitzDistribution : MicrofacetDistributionBase
    {
        private readonly float _alphaX;
        private readonly float _alphaY;

        public TrowbridgeReitzDistribution(float alphaX, float alphaY, bool sampleVisibleArea = true) 
            : base(sampleVisibleArea)
        {
            _alphaX = alphaX;
            _alphaY = alphaY;
        }

        public override float D(in Vector wh)
        {
            var tan2Theta = Tan2Theta(in wh);
            if (IsInfinity(tan2Theta))
            {
                return 0f;
            }

            var cos4Theta = Cos2Theta(in wh) * Cos2Theta(in wh);
            var e = (Cos2Theta(in wh) / (_alphaX * _alphaX) + Sin2Phi(in wh) / (_alphaY * _alphaY)) * tan2Theta;
            return 1f / (PI * _alphaX * _alphaY * cos4Theta * (1f + e) * (1f + e));
        }

        public override float Lambda(in Vector w)
        {
            var absTanTheta = Abs(TanTheta(in w));
            if (IsInfinity(absTanTheta))
            {
                return 0f;
            }

            var alpha = Sqrt(Cos2Phi(in w) * _alphaX * _alphaX + Sin2Phi(in w) * _alphaY * _alphaY);
            var alpha2Tan2Theta = alpha * absTanTheta * (alpha * absTanTheta);
            return -1f * Sqrt(1f + alpha2Tan2Theta) / 2f;
        }

        public override Vector SampleWh(in Vector wo, in Vector wi) => throw new NotImplementedException();

        public override float Pdf(in Vector wo, in Vector wh) => throw new NotImplementedException();
    }
}