using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILAnalyze
    {
        public static void Analyze(ref SAILData data)
        {
            SelfCalculation(ref data);
            SplitTemporaryVariable(data);
        }

        private static void SelfCalculation(ref SAILData data)
        {
            var lines = data.calculationLines;
            foreach (var t in lines)
            {
                var line = t;
                // Tips: self calculate need to be the last step
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
            HashSet<SAILVariableToken> equalsLeft = new HashSet<SAILVariableToken>();
            int count = 0;
            for (int i = 0; i < data.calculationLines.Count; i++)
            {
                var firstToken = data.calculationLines[i].hTokens[0].token;
                if (firstToken is SAILPieceVariableToken pieceVariableToken)
                {
                    if(!data.MatchTemporaryVariable(pieceVariableToken.link))
                        continue;
                    if (equalsLeft.Contains(pieceVariableToken.link) && !data.calculationLines[i].isSelfCalculate)
                    {
                        SAILPieceVariableToken newPieceVariableToken = new SAILPieceVariableToken();
                        newPieceVariableToken.channel = pieceVariableToken.channel;
                        newPieceVariableToken.link = pieceVariableToken.link.Copy();
                        newPieceVariableToken.link.tokenString = pieceVariableToken.link.tokenString+$"_{count}";
                        newPieceVariableToken.tokenString = newPieceVariableToken.link.tokenString + "." + newPieceVariableToken.channel;
                        count++;
                        ReplaceLinkStartFromLine(data, pieceVariableToken.link, newPieceVariableToken.link, i);
                        data.tempVar.Add(newPieceVariableToken.link);
                    }
                    else
                    {
                        equalsLeft.Add(pieceVariableToken.link);
                    }
                }
                else if(firstToken is SAILVariableToken variableToken)
                {
                    if(!data.MatchTemporaryVariable(variableToken))
                        continue;
                    if (equalsLeft.Contains(variableToken) && !data.calculationLines[i].isSelfCalculate)
                    {
                        SAILVariableToken newVariableToken = variableToken.Copy();
                        newVariableToken.tokenString += $"_{count}";
                        count++;
                        ReplaceLinkStartFromLine(data, variableToken, newVariableToken, i);
                        data.tempVar.Add(newVariableToken);
                    }
                    else
                    {
                        equalsLeft.Add(variableToken);
                    }
                }
            }
        }

        private static void ReplaceLinkStartFromLine(SAILData data, SAILVariableToken from, SAILVariableToken to, int i)
        {
            //replace all lint to sailVariableToken from calculationLines[i]
            for (int j = i; j < data.calculationLines.Count; j++)
            {
                var line = data.calculationLines[j];
                for (var index = 0; index < line.hTokens.Length; index++)
                {
                    // SAILVariableToken hToken;
                    if(line.hTokens[index].token is SAILPieceVariableToken pieceVariableToken)
                    {
                        if (ReferenceEquals(pieceVariableToken.link, from))
                        {
                            line.hTokens[index].token = to;
                        }
                    }
                    else if (line.hTokens[index].token is SAILVariableToken)
                    {
                        if (ReferenceEquals(line.hTokens[index].token, from))
                        {
                            line.hTokens[index].token = to;
                        }
                    }
                    
                }
            }
        }
    }
}