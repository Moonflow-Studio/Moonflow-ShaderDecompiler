namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILAnalyze
    {
        public static void Analyze(ref SAILData data)
        {
            SelfCalculation(ref data);
        }

        private static void SelfCalculation(ref SAILData data)
        {
            var lines = data.calculationLines;
            foreach (var t in lines)
            {
                var line = t;
                AnalyzeSelfCalculate(ref line, ref data);
            }
        }
        private static void AnalyzeSelfCalculate(ref SAILSingleline line, ref SAILData data)
        {
            //tokentype of first token is tempDeclarRegex and appears atleast twice in this line
            //then this line is self calculate
            var firstToken = line.hTokens[0].token;
            if (firstToken is SAILPieceVariableToken pieceVariableToken)
            {
                if(!data.MatchTemporaryVariable(pieceVariableToken.link))
                    return;
                int count = 0;
                foreach (var hToken in line.hTokens)
                {
                    if (hToken.token is SAILPieceVariableToken pieceHVariableToken)
                    {
                        if (ReferenceEquals(pieceHVariableToken.link, pieceVariableToken.link) && pieceHVariableToken.channel == pieceVariableToken.channel)
                        {
                            count++;
                        }
                    }
                }
                line.isSelfCalculate = count >= 2;
            }
            else if(firstToken is SAILVariableToken variableToken)
            {
                if(!data.MatchTemporaryVariable(variableToken))
                    return;
                int count = 0;
                foreach (var hToken in line.hTokens)
                {
                    if (hToken.token is SAILVariableToken hVariable)
                    {
                        if (ReferenceEquals(hVariable, variableToken))
                        {
                            count++;
                        }
                    }
                }
                line.isSelfCalculate = count >= 2;
            }
        }
        
        public static void SplitTemporaryVariable(SAILData data)
        {
            
        }
    }
}