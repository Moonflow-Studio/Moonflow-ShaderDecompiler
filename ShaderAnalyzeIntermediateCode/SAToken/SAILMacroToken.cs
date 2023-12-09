namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILMacroToken :SAILToken
    {
        public SAILMacroTokenType macroTokenType;
        public string macroName;
        private static readonly string[] macroStrings = {"#if", "#ifdef", "#ifndef", "#else", "#elif", "#endif", "#define", "#undef", "#error"};
        public string GetName()
        {
            return macroStrings[(int) macroTokenType];
        }
        // public string value;
    }
}