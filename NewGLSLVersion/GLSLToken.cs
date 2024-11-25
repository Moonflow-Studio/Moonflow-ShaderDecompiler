namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLToken
    {
        public GLSLLexer.GLSLTokenType type;
        public string tokenString;
        public bool isNegative;
        public GLSLToken(GLSLLexer.GLSLTokenType type, string toString, bool isNegative = false)
        {
            this.type = type;
            this.tokenString = toString;
            this.isNegative = isNegative;
        }

        public string ShowString()
        {
            return (isNegative ? "-" : "") + tokenString;
        }

        public float GetDisplaySize()
        {
            switch (type)
            {
                case GLSLLexer.GLSLTokenType.space: return 0;
                case GLSLLexer.GLSLTokenType.symbol: return 20;
                default:return 20 + tokenString.Length * 8;
            }
        }
    }
}