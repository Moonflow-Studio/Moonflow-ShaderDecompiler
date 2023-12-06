namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLToken
    {
        public GLSLLexer.GLSLTokenType type;
        public string tokenString;
        public GLSLToken(GLSLLexer.GLSLTokenType type, string toString)
        {
            this.type = type;
            this.tokenString = toString;
        }
    }
}