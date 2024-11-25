namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class SAILLogicalToken :SAILToken
    {
        private int logicalOperatorIndex;
        private static readonly string[] logicalOperatorStrings = new[] {"if","else","discard","return","break","continue","for"};

        public void Init(string str)
        {
            for (int i = 0; i < logicalOperatorStrings.Length; i++)
            {
                if (str == logicalOperatorStrings[i]) logicalOperatorIndex = i;
            }
        }
        public string GetName()
        {
            return logicalOperatorStrings[logicalOperatorIndex];
        }
    }
}