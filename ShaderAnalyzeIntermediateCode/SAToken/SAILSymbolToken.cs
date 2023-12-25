namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class SAILSymbolToken:SAILToken
    {
        private static readonly char[] symbolStrings = new []{'+','-','*','/','<','>','(',')','{','}','[',']',';',',','.'};
        public override float GetDisplaySize()
        {
            return 20;
        }
    }
}