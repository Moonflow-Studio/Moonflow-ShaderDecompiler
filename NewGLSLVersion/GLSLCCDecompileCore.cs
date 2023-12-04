using System.Collections.Generic;
using UnityEngine.Serialization;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLCCDecompileCore
    {
        public List<SingleLine> lines;
        public struct SingleLine
        {
            public string lineString;
            public HierarchicalToken[] hTokens;
            public LineType lineType;
            public bool isSelfCalculate;
        }

        public struct HierarchicalToken
        {
            public GLSLCCToken token;
            public int layer;
        }

        public enum LineType
        {
            inoutDeclaration,
            uniformDeclaration,
            calculate,
            macro,
            others
        }

        public List<SingleLine> SplitToLines(ref List<GLSLCCToken> tokens)
        {
            //split by ';'
            var lines = new List<SingleLine>();
            var line = new SingleLine();
            var lineTokens = new List<HierarchicalToken>();
            char[] wrapChars = {'\n', '\f'};
            int currentLayer = 0;
            string lineString = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.type == GLSLLexer.TokenType.space && token.tokenString.IndexOfAny(wrapChars) != -1)
                {
                    line.hTokens = lineTokens.ToArray();
                    line.lineString = lineString;
                    lineTokens.Clear();
                    lines.Add(line);
                    line = new SingleLine();
                    lineString = "";
                }
                else
                {
                    lineString += token.tokenString;
                    if(token.tokenString != " ")
                        lineTokens.Add(new HierarchicalToken(){layer = currentLayer, token = token});
                    
                }
                if(token.type == GLSLLexer.TokenType.symbol && token.tokenString == "(")
                {
                    currentLayer++;
                }
                if(token.type == GLSLLexer.TokenType.symbol && token.tokenString == ")")
                {
                    currentLayer--;
                }
            }
            return lines;
        }
        
        public void Analyze(ref SingleLine line)
        {
            AnalyzeLineType(ref line);
            if(line.lineType == LineType.calculate)
                AnalyzeSelfCalculate(ref line);
        }

        private void AnalyzeSelfCalculate(ref SingleLine line)
        {
            //tokentype of first token is tempDeclarRegex and appears atleast twice in this line
            //then this line is self calculate
            var firstToken = line.hTokens[0].token;
            if (firstToken.type != GLSLLexer.TokenType.tempDeclarRegex) return;
            int count = 0;
            foreach (var hToken in line.hTokens)
            {
                if (hToken.token.type == GLSLLexer.TokenType.tempDeclarRegex && hToken.token.tokenString == firstToken.tokenString)
                {
                    count++;
                }
            }
            line.isSelfCalculate = count >= 2;
        }

        private void AnalyzeLineType(ref SingleLine line)
        {
            if(line.hTokens == null || line.hTokens.Length == 0) return;
            //first token is macros
            if (line.hTokens[0].token.type == GLSLLexer.TokenType.macros)
            {
                line.lineType = LineType.macro;
                return;
            }
            //first token is uniform/in/out
            if (line.hTokens[0].token.type == GLSLLexer.TokenType.inputModifier)
            {
                line.lineType = line.hTokens[0].token.tokenString == "uniform" ? LineType.uniformDeclaration : LineType.inoutDeclaration;
                return;
            }
            //first token is calculate
            if (line.hTokens.Length > 1)
            {
                if (line.hTokens[1].token.type == GLSLLexer.TokenType.symbol && line.hTokens[1].token.tokenString == "=")
                {
                    line.lineType = LineType.calculate;
                    return;
                }
            }
            line.lineType = LineType.others;
        }
    }
}