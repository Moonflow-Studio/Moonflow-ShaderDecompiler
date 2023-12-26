using System.Collections.Generic;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILAnalyze
    {
        public static void Analyze(ref SAILData data)
        {
            SelfCalculation(ref data);
            SplitTemporaryVariable(data);
            //TODO: 把带有括号的表达式拆分成多行
            SelfCalculation(ref data);
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
            HashSet<SAILPieceVariableToken> equalsLeft = new HashSet<SAILPieceVariableToken>();
            int count = 0;
            for (int i = 0; i < data.calculationLines.Count; i++)
            {
                var firstToken = data.calculationLines[i].hTokens[0].token;
                if (firstToken is SAILPieceVariableToken pieceVariableToken)
                {
                    if(!data.MatchTemporaryVariable(pieceVariableToken.link))
                        continue;
                    if (MatchPieceVariable(equalsLeft, pieceVariableToken) && !data.calculationLines[i].isSelfCalculate)
                    {
                        SAILPieceVariableToken newPieceVariableToken = new SAILPieceVariableToken();
                        newPieceVariableToken.channel = pieceVariableToken.channel;
                        newPieceVariableToken.link = pieceVariableToken.link.Copy();
                        newPieceVariableToken.link.tokenString = pieceVariableToken.link.tokenString+$"_{count}";
                        newPieceVariableToken.tokenString = newPieceVariableToken.link.tokenString + "." + newPieceVariableToken.channel;
                        count++;
                        ReplaceLinkStartFromLine(data, pieceVariableToken, newPieceVariableToken, i, newPieceVariableToken.channel);
                        data.tempVar.Add(newPieceVariableToken.link);
                    }
                    else
                    {
                        equalsLeft.Add(pieceVariableToken);
                    }
                }
                // else if(firstToken is SAILVariableToken variableToken)
                // {
                //     if(!data.MatchTemporaryVariable(variableToken))
                //         continue;
                //     if (MatchFullVariable(equalsLeft, variableToken) && !data.calculationLines[i].isSelfCalculate)
                //     {
                //         SAILVariableToken newVariableToken = variableToken.Copy();
                //         newVariableToken.tokenString += $"_{count}";
                //         count++;
                //         ReplaceLinkStartFromLine(data, variableToken, newVariableToken, i);
                //         data.tempVar.Add(newVariableToken);
                //     }
                //     else
                //     {
                //         equalsLeft.Add(new SAILPieceVariableToken()
                //         {
                //             channel = "",
                //             link = variableToken,
                //             tokenString = variableToken.ShowString()
                //         });
                //     }
                // }
            }
        }

        private static bool MatchPieceVariable(HashSet<SAILPieceVariableToken> pieces, SAILPieceVariableToken waitForMatch)
        {
            foreach (var piece in pieces)
            {
                if (piece.channel == waitForMatch.channel && ReferenceEquals(piece.link, waitForMatch.link))
                    return true;
            }
            return false;
        }

        private static bool MatchFullVariable(HashSet<SAILPieceVariableToken> pieces, SAILVariableToken waitForMatch)
        {
            foreach (var piece in pieces)
            {
                if (piece.channel == "" && ReferenceEquals(piece.link, waitForMatch))
                    return true;
            }
            return false;
        }

        private static void ReplaceLinkStartFromLine(SAILData data, SAILPieceVariableToken from, SAILPieceVariableToken to, int i, string channel = "")
        {
            //replace all lint to sailVariableToken from calculationLines[i]
            for (int j = i; j < data.calculationLines.Count; j++)
            {
                var line = data.calculationLines[j];
                for (var index = 0; index < line.hTokens.Count; index++)
                {
                    // SAILVariableToken hToken;
                    var sailHierToken = line.hTokens[index];
                    if(sailHierToken.token is SAILPieceVariableToken pieceVariableToken)
                    {
                        if (ReferenceEquals(pieceVariableToken.link, from.link) && pieceVariableToken.MatchChannel(from.channel, out bool totalMatch))
                        {
                            if (totalMatch)
                            {
                                pieceVariableToken.link = to.link;
                                pieceVariableToken.tokenString = to.tokenString + '.' + pieceVariableToken.channel;
                            }
                            else
                            {
                                //TODO:与上文替换的片段参数有重合通道但不是所有通道都重合度需要重新拆分成多个片段
                            }
                        }
                    }
                    // else if (sailHierToken.token is SAILVariableToken variableToken)
                    // {
                    //     if (ReferenceEquals(variableToken, from.link))
                    //     {
                    //         variableToken = to;
                    //         variableToken.tokenString = to.tokenString;
                    //     }
                    // }
                }
            }
        }
    }
}