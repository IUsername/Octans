using System;
using static System.MathF;
using static System.Single;
using static Octans.MathF;

namespace Octans.Reflection.Microfacet
{
    public sealed class BeckmannDistribution : MicrofacetDistributionBase
    {
        private readonly float _alphaX;
        private readonly float _alphaY;

        public BeckmannDistribution(float alphaX, float alphaY, bool sampleVisibleArea = true)
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

        public override Vector SampleWh(in Vector wo, in Vector wi) => throw new NotImplementedException();

        public override float Pdf(in Vector wo, in Vector wh) => throw new NotImplementedException();
    }
}