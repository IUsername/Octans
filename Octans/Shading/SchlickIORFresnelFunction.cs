using System;

namespace Octans.Shading
{
    public class SchlickIORFresnelFunction : IFresnelFunction
    {
        public static IFresnelFunction Instance = new SchlickIORFresnelFunction();

        private SchlickIORFresnelFunction()
        {
        }

        public float Factor(in ShadingInfo info) => Calc(info.IoR, info.LdotH);

        private static float SchlickFresnelFunc(float i)
        {
            var x = MathFunction.Saturate(1f - i);
            var x2 = x * x;
            return x2 * x2 * x;
        }

        private static float Calc(float ior, float LdotH)
        {
            var f0 = MathF.Pow(ior - 1f, 2f) / MathF.Pow(ior + 1f, 2f);
            return f0 + (1 - f0) * SchlickFresnelFunc(LdotH);
        }
    }
}