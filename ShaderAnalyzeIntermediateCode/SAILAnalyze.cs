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
            // HashSet<SAILPieceVariableToken> equalsLeft = new HashSet<SAILPieceVariableToken>();
            Dictionary<SAILPieceVariableToken, int> equalsLeft = new Dictionary<SAILPieceVariableToken, int>();
            int count = 0;
            int branchLayer = 0;
            for (int i = 0; i < data.calculationLines.Count; i++)
            {
                var firstToken = data.calculationLines[i].hTokens[0].token;
                if (firstToken is SAILPieceVariableToken pieceVariableToken)
                {
                    if(!data.MatchTemporaryVariable(pieceVariableToken.link) || branchLayer > 0)
                        continue;
                    //TODO: 替换条件：1.行不在分支里（branchLayer==0) 2.等号左侧变量所有通道都被更新
                    if (!data.calculationLines[i].isSelfCalculate && branchLayer == 0 && MatchPieceVariable(equalsLeft, pieceVariableToken) && pieceVariableToken.channel == pieceVariableToken.link.GetDefaultChannel())
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
                        //强制更新等号左侧变量的分支状态
                        if (equalsLeft.ContainsKey(pieceVariableToken))
                        {
                            equalsLeft[pieceVariableToken] = branchLayer;
                        }
                        else
                        {
                            equalsLeft.Add(pieceVariableToken, branchLayer);
                        }
                    }
                }
                else if (firstToken is SAILMacroToken macroToken)
                {
                    if (macroToken.macroTokenType is SAILMacroTokenType.IF or SAILMacroTokenType.IFDEF or SAILMacroTokenType.IFNDEF)
                    {
                        branchLayer += 1;
                    }else if (macroToken.macroTokenType == SAILMacroTokenType.ENDIF)
                    {
                        branchLayer -= 1;
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

        private static bool MatchPieceVariable(Dictionary<SAILPieceVariableToken, int> pieces, SAILPieceVariableToken waitForMatch)
        {
            foreach (var piece in pieces)
            {
                //参数上次出现不能在分支内，否则替换会有问题
                if (piece.Key.channel == waitForMatch.channel && ReferenceEquals(piece.Key.link, waitForMatch.link) && piece.Value == 0)
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
            //替换是完全替换的（不论channel数量、branch）
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
                            if (totalMatch/* || pieceVariableToken.channel.Length == 1*/)
                            {
                                sailHierToken.token = new SAILPieceVariableToken()
                                {
                                    channel = from.channel,
                                    link = to.link,
                                    tokenString = to.tokenString + '.' + to.channel
                                };
                                line.hTokens[index] = sailHierToken;
                            }
                            else
                            {
                                //TODO:与上文替换的片段参数有重合通道但不是所有通道都重合度需要重新拆分成多个片段
                                if (index > 0)
                                {
                                    int startIndex = index;
                                    char[] channels = pieceVariableToken.channel.ToCharArray();
                                    // int replacedHTokenSize = channels.Length * 2 + 2;// n通道Token + (n-1)个逗号Token + 数据类型Token + 左右括号Token
                                    SAILHierToken dataTypeHT = new SAILHierToken()
                                    {
                                        isNegative = line.hTokens[startIndex].isNegative,
                                        layer = line.hTokens[startIndex].layer,
                                        token = SAILTokenFactory.CreateVariable(
                                            pieceVariableToken.link.tokenType.ToString().ToLower(),
                                            pieceVariableToken.link.tokenType)
                                    };
                                    line.hTokens.RemoveAt(startIndex);
                                    line.hTokens.Insert(startIndex, dataTypeHT);
                                    startIndex++;
                                    SAILHierToken leftBracket = new SAILHierToken()
                                        { token = new SAILSymbolToken() { tokenString = "(" } };
                                    line.hTokens.Insert(startIndex, leftBracket);
                                    startIndex++;
                                    for (int k = 0; k < channels.Length; k++)
                                    {
                                        //from里有的通道赋值为to，from里没有的通道赋值为from
                                        bool matched = from.MatchChannel(channels[k].ToString(), out bool trash);
                                        SAILHierToken channelK = new SAILHierToken()
                                        {
                                            isNegative = false,
                                            layer = line.hTokens[startIndex].layer + 1,
                                            token = new SAILPieceVariableToken()
                                            {
                                                channel = channels[k].ToString(),
                                                link = matched ? to.link : from.link,
                                                tokenString = to.link.tokenString + '.' + channels[k]
                                            }
                                        };
                                        line.hTokens.Insert(startIndex, channelK);
                                        startIndex++;

                                        if (k < channels.Length - 1)
                                        {
                                            SAILHierToken Comma = new SAILHierToken()
                                                { token = new SAILSymbolToken() { tokenString = "," } };
                                            line.hTokens.Insert(startIndex, Comma);
                                            startIndex++;
                                        }
                                    }
                                    SAILHierToken rightBracket = new SAILHierToken()
                                        { token = new SAILSymbolToken() { tokenString = ")" } };
                                    line.hTokens.Insert(startIndex, rightBracket);
                                }
                                else
                                {
                                    line.hTokens[index] = new SAILHierToken()
                                    {
                                        isNegative = false,
                                        layer = 0,
                                        token = new SAILPieceVariableToken()
                                        {
                                            channel = pieceVariableToken.channel,
                                            link = to.link,
                                            tokenString = to.tokenString + "." + pieceVariableToken.channel
                                        }
                                    };
                                }
                                
                            }
                        }
                    }
                    else if (sailHierToken.token is SAILVariableToken variableToken)
                    {
                        if (ReferenceEquals(variableToken, from.link))
                        {
                            variableToken = to.link;
                            variableToken.tokenString = to.tokenString;
                        }
                    }
                }
            }
        }
    }
}