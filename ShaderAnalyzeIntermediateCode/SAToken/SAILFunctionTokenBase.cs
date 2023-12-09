namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILFunctionTokenBase : SAILToken
    {
        private int funcIndex = -1;
        private static readonly string[] functionString = new[]
        {
            "abort", "abs", "acos", "all", "any", "asin", "asint", "asuint", "atan", "atan2", "ceil", "clamp", "clip",
            "cos", /*"cosh", "countbits",*/ "cross", "ddx", /*"ddx_coarse", "ddx_fine",*/ "ddy", /*"ddy_coarse", "ddy_fine",
            "degrees", "determinant",*/ "distance", "dot", "dst", /*"errorf",*/ "exp", "exp2", "floor", "fma", "fmod", "frac",
            "frexp", "fwidth", /*"isfinite", "isinf", "isnan", "ldexp",*/
            "length", "lerp", /*"lit",*/ "log", "log10", "log2", /*"mad",*/ "max", "min", "modf", "msad4", "mul", /*"normalize",*/
            "pow", /*"radians",*/ "rcp", "reflect", "refract", "reversebits", "round", "rsqrt", /*"saturate",*/ "sign", "sin",
            "sincos", "sinh", /*"smoothstep",*/ "sqrt", "step", "tan", "tanh", /* "transpose",*/"sampleTexture",
            "trunc", /*"InterlockedAdd", "InterlockedAnd", "InterlockedCompareExchange", "InterlockedCompareStore",
            "InterlockedExchange", "InterlockedMax", "InterlockedMin", "InterlockedOr", "InterlockedXor",*/"ldexp", 
        };
        //TODO: 匹配function的参数数量
        public bool Init(string str)
        {
            for (int i = 0; i < functionString.Length; i++)
            {
                if (str == functionString[i])
                {
                    funcIndex = i;
                    return true;
                }
            }
            return false;
        }
        
        public string GetName()
        {
            if(funcIndex != -1)
                return functionString[funcIndex];
            return "NULL Function";
        }
    }
}