namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLCCToken
    {
        public GLSLLexer.TokenType type;
        public string tokenString;
        public GLSLCCToken(GLSLLexer.TokenType type, string toString)
        {
            this.type = type;
            this.tokenString = toString;
        }
    }
}