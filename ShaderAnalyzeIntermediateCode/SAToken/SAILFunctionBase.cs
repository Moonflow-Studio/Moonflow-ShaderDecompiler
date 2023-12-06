namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILFunctionBase
    {
        private static readonly string[] functionString = new[]
        {
            "abort", "abs", "acos", "all", "any", "asin", "asint", "asuint", "atan", "atan2", "ceil", "clamp", "clip",
            "cos", /*"cosh", "countbits",*/ "cross", "ddx", /*"ddx_coarse", "ddx_fine",*/ "ddy", /*"ddy_coarse", "ddy_fine",
            "degrees", "determinant",*/ "distance", "dot", "dst", /*"errorf",*/ "exp", "exp2", "floor", "fma", "fmod", "frac",
            "frexp", "fwidth", /*"isfinite", "isinf", "isnan", "ldexp",*/
            "length", "lerp", /*"lit",*/ "log", "log10", "log2", /*"mad",*/ "max", "min", "modf", "msad4", "mul", /*"normalize",*/
            "pow", /*"radians",*/ "rcp", "reflect", "refract", "reversebits", "round", "rsqrt", /*"saturate",*/ "sign", "sin",
            "sincos", "sinh", /*"smoothstep",*/ "sqrt", "step", "tan", "tanh", "tex1D", "tex1Dbias", "tex1Dgrad",
            "tex1Dlod",
            "tex1Dproj", "tex2D", "tex2Dbias", "tex2Dgrad", "tex2Dlod", "tex2Dproj", "tex3D", "tex3Dbias", "tex3Dgrad",
            "tex3Dlod", "tex3Dproj", "texCUBE", "texCUBEbias", "texCUBEgrad", "texCUBElod", "texCUBEproj",/* "transpose",*/
            "trunc", /*"InterlockedAdd", "InterlockedAnd", "InterlockedCompareExchange", "InterlockedCompareStore",
            "InterlockedExchange", "InterlockedMax", "InterlockedMin", "InterlockedOr", "InterlockedXor",*/"ldexp", 
        };
    }
}