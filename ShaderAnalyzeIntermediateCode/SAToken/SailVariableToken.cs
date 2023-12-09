namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILVariableToken : SAILToken
    {
        public string tokenTypeName;
    }
    public class SailVariableToken<T> : SAILVariableToken
    {
        public T data;
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