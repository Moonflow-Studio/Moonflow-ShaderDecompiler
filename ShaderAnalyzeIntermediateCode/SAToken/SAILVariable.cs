namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILVariable : SAILToken
    {
    }
    public class SAILVariable<T> : SAILVariable
    {
        public T data;
    }
    public class SAILTexture : SAILVariable
    {
    }
    public class SAILVariablePiece : SAILToken
    {
        public string channel;
        public SAILVariable link;
    }
}