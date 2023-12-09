namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILSampleFunctionToken : SAILToken
    {
        public int sampleFuncIndex;
        private static readonly string[] sampleFunctionString = new[]
        {
            "tex1D", "tex1Dbias", "tex1Dgrad", "tex1Dlod", "tex1Dproj", "tex2D", "tex2Dbias", "tex2Dgrad", "tex2Dlod",
            "tex2Dproj", "tex3D", "tex3Dbias", "tex3Dgrad", "tex3Dlod", "tex3Dproj", "texCUBE", "texCUBEbias",
            "texCUBEgrad", "texCUBElod", "texCUBEproj"
        };
        public bool InitSampleFunction(string str)
        {
            for (int i = 0; i < sampleFunctionString.Length; i++)
            {
                if (str == sampleFunctionString[i])
                {
                    sampleFuncIndex = i;
                    return true;
                }
            }
            return false;
        }
        
    }
}