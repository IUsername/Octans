using System;

namespace Octans.Shading
{
    public class SchlickFresnelFunction : IFresnelFunction
    {
        public static IFresnelFunction Instance = new SchlickFresnelFunction();

        private SchlickFresnelFunction()
        {
        }

        //private static float SchlickFresnelFunc(float i)
        //{
        //    var x = MathFunction.ClampF(0f, 1f, 1f - i);
        //    var x2 = x * x;
        //    return x2 * x2 * x;
        //}

        //private static Color Calc(in Color specularColor, float LdotH)
        //{
        //    return specularColor + (Colors.White - specularColor) * SchlickFresnelFunc(LdotH);
        //}

        public float Factor(in ShadingInfo info) => info.F0 + (1f - info.F0) * MathF.Pow(1f - info.LdotH, 5);
    }
}