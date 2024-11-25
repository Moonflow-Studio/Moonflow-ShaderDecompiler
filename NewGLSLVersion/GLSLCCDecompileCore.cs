using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Moonflow.Tools.MFUtilityTools.GLSLCC
{
    public class GLSLCCDecompileCore
    {
        public List<GLSLSingleLine> originLines;
        
        public struct GLSLSingleLine
        {
            public string lineString;
            public GLSLToken[] tokens;
            public GLSLLineType glslLineType;
            // public bool isSelfCalculate;
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
            // if(line.glslLineType == GLSLLineType.calculate)
                // AnalyzeSelfCalculate(ref line);
        }

        private void AnalyzeLineType(ref GLSLSingleLine line)
        {
            //TODO:重做识别行类型
            //由于不同编译器会产生不同布局，所以这里只能通过识别关键字来判断行类型
            //目前确认通用的逻辑有：
            //1.第一个token是 macroStrings中的字符串，则该行一定是宏定义
                //1.1 如果宏定义为#define，则有可能下文中会出现无法识别的函数，来自于宏定义的函数，需要先把宏定义的函数识别出来
            //2.第一个token是InputModifierStrings中的字符串，则该行一定是非临时变量的声明
            //3.第一个token是logicalOperatorStrings中的字符串，则该行一定是逻辑语句
            //4.第二个token是"="，则该行一定是计算语句
            //5.倒数第三个token是dataTypes中的字符串，则该行一定是临时变量的声明
            
            //目前发现编译器有差异的内容有：
            //有的编译器会按照输入的分行策略进行编译，有的编译器会把输入时单行多项操作拆成多行
            //有的编译器会把临时变量声明放在计算语句前面，有的编译器会把临时变量声明放在计算语句后面
            //有的编译器不会把函数解开，可能存在函数调用
            
            
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