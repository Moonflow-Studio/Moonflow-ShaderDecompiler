using System.Collections.Generic;
using UnityEngine.Serialization;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLCCDecompileCore
    {
        public List<GLSLSingleLine> originLines;
        
        public struct GLSLSingleLine
        {
            public string lineString;
            public GLSLToken[] tokens;
            public GLSLLineType glslLineType;
            public bool isSelfCalculate;
        }

        // public struct GLSLHierarchicalToken
        // {
        //     public GLSLToken token;
        //     public int layer;
        // }

        public enum GLSLLineType
        {
            declaration,
            tempDeclaration,
            logic,
            calculate,
            macro,
            others
        }

        public List<GLSLSingleLine> SplitToLines(ref List<GLSLToken> tokens)
        {
            //split by ';'
            var lines = new List<GLSLSingleLine>();
            var line = new GLSLSingleLine();
            var lineTokens = new List<GLSLToken>();
            char[] wrapChars = {'\n', '\f'};
            // int currentLayer = 0;
            string lineString = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.type == GLSLLexer.GLSLTokenType.space && token.tokenString.IndexOfAny(wrapChars) != -1)
                {
                    line.tokens = lineTokens.ToArray();
                    line.lineString = lineString;
                    lineTokens.Clear();
                    lines.Add(line);
                    line = new GLSLSingleLine();
                    lineString = "";
                }
                else
                {
                    lineString += token.tokenString;
                    if(token.tokenString != " ")
                        lineTokens.Add(token);
                    
                }
            }
            return lines;
        }
        
        public void Analyze(ref GLSLSingleLine line)
        {
            AnalyzeLineType(ref line);
            if(line.glslLineType == GLSLLineType.calculate)
                AnalyzeSelfCalculate(ref line);
        }

        private void AnalyzeSelfCalculate(ref GLSLSingleLine line)
        {
            //tokentype of first token is tempDeclarRegex and appears atleast twice in this line
            //then this line is self calculate
            var firstToken = line.tokens[0];
            if (firstToken.type != GLSLLexer.GLSLTokenType.tempDeclarRegex) return;
            int count = 0;
            foreach (var hToken in line.tokens)
            {
                if (hToken.type == GLSLLexer.GLSLTokenType.tempDeclarRegex && hToken.tokenString == firstToken.tokenString)
                {
                    count++;
                }
            }
            line.isSelfCalculate = count >= 2;
        }

        private void AnalyzeLineType(ref GLSLSingleLine line)
        {
            if(line.tokens == null || line.tokens.Length == 0) return;
            //first token is macros
            if (line.tokens[0].type == GLSLLexer.GLSLTokenType.macros)
            {
                line.glslLineType = GLSLLineType.macro;
                return;
            }
            //first token is uniform/in/out
            if (line.tokens[0].type == GLSLLexer.GLSLTokenType.inputModifier)
            {
                line.glslLineType = GLSLLineType.declaration;
                return;
            }
            //first token is logic
            if (line.tokens[0].type == GLSLLexer.GLSLTokenType.logicalOperator)
            {
                line.glslLineType = GLSLLineType.logic;
                return;
            }
            //first token is calculate
            if (line.tokens.Length > 1)
            {
                if (line.tokens[1].type == GLSLLexer.GLSLTokenType.symbol && line.tokens[1].tokenString == "=")
                {
                    line.glslLineType = GLSLLineType.calculate;
                    return;
                }
            }
            //the one before the last token is tempDeclarRegex
            if (line.tokens.Length > 3 && line.tokens[^3].type == GLSLLexer.GLSLTokenType.tempDeclarRegex)
            {
                line.glslLineType = GLSLLineType.tempDeclaration;
                return;
            }
            line.glslLineType = GLSLLineType.others;
        }
        
        
    }
}