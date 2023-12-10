namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILVariableToken : SAILToken
    {
        public string tokenTypeName;

        public virtual SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken();
            copy.tokenString = tokenString;
            copy.tokenTypeName = tokenTypeName;
            return copy;
        }
    }
    public class SAILVariableToken<T> : SAILVariableToken
    {
        public T data;
        public override SAILVariableToken Copy()
        {
            var copy = new SAILVariableToken<T>();
            copy.tokenString = tokenString;
            copy.tokenTypeName = tokenTypeName;
            copy.data = data;
            return copy;
        }
    }
    public class SAILTextureToken : SAILVariableToken
    {
        public SAILTextureType type;
        public SAILSamplerType samplerType;
    }
    public class SAILPieceVariableToken : SAILToken
    {
        public string channel;
        public SAILVariableToken link;
        public override string ShowString()
        {
            return link.tokenString + "." + channel;
        }
    }
}