using System;
using System.Collections.Generic;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class GLSL2SAIL
    {
        public static SAILData TransToSAIL(ref List<GLSLCCDecompileCore.GLSLSingleLine> lines)
        {
            var sailData = new SAILData();
            bool finishDeclaration = false;
            for (int i = 0; i < lines.Count; i++)
            {
                GLSLCCDecompileCore.GLSLSingleLine line = lines[i];
                if(line.tokens == null || line.tokens.Length == 0) continue;
                switch (line.glslLineType)
                {
                    case GLSLCCDecompileCore.GLSLLineType.declaration:
                        if (finishDeclaration) Debug.Assert(false, "计算开始后不应该再有变量定义");
                        else
                        {
                            CreateDeclaration(ref line, ref sailData);
                        }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.tempDeclaration:
                        if (finishDeclaration) Debug.Assert(false, "计算开始后不应该再有临时变量定义");
                        else
                        {
                            CreateDeclaration(ref line, ref sailData);
                        }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.calculate:
                        if (!finishDeclaration) Debug.Assert(false, "计算开始前不应有计算语句");
                        else
                        {
                            var singleline = new SAILSingleline();
                            singleline.lineString = "";
                            singleline.hTokens = new SAILHierToken[line.tokens.Length];
                            int layer = 0;
                            for (int j = 0; j < line.tokens.Length; j++)
                            {
                                var token = CreateCalculateToken(ref line.tokens[j], ref singleline, ref sailData, ref layer);
                                if(token.token == null) continue;
                                singleline.hTokens[j] = token;
                                try
                                {
                                    singleline.lineString += token.token?.tokenString;

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }
                            }
                            
                            sailData.calculationLines.Add(singleline);
                        }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.logic:
                        //TODO: 逻辑起始的行是否需要特殊逻辑
                        if (!finishDeclaration) Debug.Assert(false, "计算开始前不应有逻辑语句");
                        else
                        {
                            var singleline = new SAILSingleline();
                            singleline.lineString = "";
                            singleline.hTokens = new SAILHierToken[line.tokens.Length];
                            int layer = 0;
                            for (int j = 0; j < line.tokens.Length; j++)
                            {
                                var token = CreateCalculateToken(ref line.tokens[j], ref singleline, ref sailData, ref layer);
                                singleline.hTokens[j] = token;
                                singleline.lineString += token.token?.tokenString;
                            }
                        }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.macro:
                        if(!finishDeclaration) continue;//TODO:计算开始之前的macro
                        var macroLine = new SAILSingleline()
                        {
                            hTokens = new[]
                            {
                                new SAILHierToken()
                                {
                                    layer = 0,
                                    token = new SAILMacroToken()
                                    {
                                        macroTokenType = MatchMacroType(line.tokens[0].tokenString),
                                        // value = line.tokens[0].tokenString
                                    }
                                },
                                new SAILHierToken()
                                {
                                    layer = 0,
                                    token = new SAILSymbolToken()
                                    {
                                        tokenString = line.lineString.Replace(line.tokens[0].tokenString, "")
                                    }
                                }
                            }
                        };
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.others:
                        if(line.tokens[0].tokenString == "void")
                        {
                            finishDeclaration = true;
                        }
                        break;
                }
            }
            return sailData;
        }

        private static SAILHierToken CreateCalculateToken(ref GLSLToken lineToken, ref SAILSingleline line, ref SAILData data, ref int layer)
        {
            SAILHierToken hToken = new SAILHierToken();
            hToken.layer = layer;
            switch (lineToken.type)
            {
                case GLSLLexer.GLSLTokenType.instrFunc:
                {
                    SAILFunctionTokenBase functionToken = new SAILFunctionTokenBase();
                    functionToken.Init(lineToken.tokenString);
                    hToken.token = functionToken;
                    //TODO: 需要读取后面layer+1的内容
                    return hToken;
                }
                case GLSLLexer.GLSLTokenType.dataType:
                {
                    var type = MatchDataType(lineToken.tokenString);
                    hToken.token = SAILTokenFactory.CreateVariable(lineToken.tokenString, type);
                    //TODO: 需要读取后面layer+1的常量
                    return hToken;
                }
                case GLSLLexer.GLSLTokenType.tempDeclarRegex:
                {
                    //analyze lineToken by '.'
                    var tmpTokens = lineToken.tokenString.Split('.');
                    //left of '.' is variable name, right of '.' is variable member
                    if (tmpTokens.Length == 1)
                    {
                        //variable name
                        hToken.token = data.FindVariable(tmpTokens[0]);
                    }
                    else
                    {
                        //variable member
                        var variable = data.FindVariable(tmpTokens[0]);
                        hToken.token = new SAILPieceVariableToken()
                        {
                            channel = tmpTokens[1],
                            link = variable,
                            tokenString = variable.tokenString
                        };
                    }
                    return hToken;
                }
                case GLSLLexer.GLSLTokenType.semanticRegex:
                    //analyze lineToken by '.'
                    var semTokens = lineToken.tokenString.Split('.');
                    //left of '.' is variable name, right of '.' is variable member
                    if (semTokens.Length == 1)
                    {
                        //variable name
                        hToken.token = data.FindVariable(semTokens[0]);
                    }
                    else
                    {
                        //variable member
                        var variable = data.FindVariable(semTokens[0]);
                        hToken.token = new SAILPieceVariableToken()
                        {
                            channel = semTokens[1],
                            link = variable,
                            tokenString = variable.tokenString
                        };
                    }
                    return hToken;
                case GLSLLexer.GLSLTokenType.number:
                    hToken.token = new SAILNumberToken()
                    {
                        tokenString = lineToken.tokenString
                    };
                    return hToken;
                case GLSLLexer.GLSLTokenType.symbol:
                    hToken.token = new SAILSymbolToken()
                    {
                        tokenString = lineToken.tokenString
                    };
                    if(lineToken.tokenString == "(")layer++;
                    if(lineToken.tokenString == ")")layer--;
                    return hToken;
                case GLSLLexer.GLSLTokenType.macros:
                    hToken.token = new SAILMacroToken()
                    {
                        macroTokenType = MatchMacroType(lineToken.tokenString),
                        // value = lineToken.tokenString
                    };
                    return hToken;
                // case GLSLLexer.GLSLTokenType.logicalOperator:
                //     SAILLogicalToken logicalToken = new SAILLogicalToken();
                //     logicalToken.Init(lineToken.tokenString);
                //     hToken.token = logicalToken;
                //     //TODO: 需要读取后面layer+1的内容
                //     return hToken;
            }
            return new SAILHierToken();
        }

        private static void CreateDeclaration(ref GLSLCCDecompileCore.GLSLSingleLine line, ref SAILData sailData)
        {
            //find data type
            int modifierType = -1;
            SAILDataTokenType dataTokenType = SAILDataTokenType.error;
            for (int j = 0; j < line.tokens.Length; j++)
            {
                if (line.tokens[j].type == GLSLLexer.GLSLTokenType.inputModifier)
                {
                    modifierType = MatchInputModifier(line.tokens[j].tokenString);
                }

                if (line.tokens[j].type == GLSLLexer.GLSLTokenType.dataType)
                {
                    dataTokenType = MatchDataType(line.tokens[j].tokenString);
                }

                if (line.tokens[j].type == GLSLLexer.GLSLTokenType.name || line.tokens[j].type == GLSLLexer.GLSLTokenType.semanticRegex || line.tokens[j].type == GLSLLexer.GLSLTokenType.tempDeclarRegex)
                {
                    if (dataTokenType != SAILDataTokenType.error)
                    {
                        switch (modifierType)
                        {
                            case 0:
                                if(dataTokenType == SAILDataTokenType.SAMPLER2D || dataTokenType == SAILDataTokenType.SAMPLER3D || dataTokenType == SAILDataTokenType.SAMPLERCUBE)
                                    sailData.glbVar.Add(SAILTokenFactory.CreateTexture(line.tokens[j].tokenString, dataTokenType));
                                else
                                    sailData.glbVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                return;
                            case 1:
                                sailData.inVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                return;
                            case 2:
                                sailData.outVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                return;
                            case 3:
                                sailData.inVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                sailData.outVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                return;
                            case -1:
                                sailData.glbVar.Add(SAILTokenFactory.CreateVariable(line.tokens[j].tokenString, dataTokenType));
                                return;
                        }
                    }
                }
            }
            Debug.LogError($"未知的声明: {line.lineString} - modifierType:{modifierType} dataType:{dataTokenType}");
        }

        private static SAILMacroTokenType MatchMacroType(string str)
        {
            if(str == "#ifdef") return SAILMacroTokenType.IFDEF;
            if(str == "#ifndef") return SAILMacroTokenType.IFNDEF;
            if(str == "#else") return SAILMacroTokenType.ELSE;
            if(str == "#endif") return SAILMacroTokenType.ENDIF;
            if(str == "#define") return SAILMacroTokenType.DEFINE;
            if(str == "#undef") return SAILMacroTokenType.UNDEF;
            if(str == "#if") return SAILMacroTokenType.IF;
            if(str == "#elif") return SAILMacroTokenType.ELIF;
            return SAILMacroTokenType.ERROR;
        }

        private static SAILDataTokenType MatchDataType(string str)
        {
            if (str == "float") return SAILDataTokenType.FLOAT;
            if (str == "int") return SAILDataTokenType.INT;
            if (str == "uint") return SAILDataTokenType.UINT;
            if (str == "bool") return SAILDataTokenType.BOOL;
            if (str == "vec2") return SAILDataTokenType.FLOAT2;
            if (str == "vec3") return SAILDataTokenType.FLOAT3;
            if (str == "vec4") return SAILDataTokenType.FLOAT4;
            if (str == "ivec2") return SAILDataTokenType.INT2;
            if (str == "ivec3") return SAILDataTokenType.INT3;
            if (str == "ivec4") return SAILDataTokenType.INT4;
            if (str == "uvec2") return SAILDataTokenType.UINT2;
            if (str == "uvec3") return SAILDataTokenType.UINT3;
            if (str == "uvec4") return SAILDataTokenType.UINT4;
            if (str == "sampler2D") return SAILDataTokenType.SAMPLER2D;
            if (str == "sampler3D") return SAILDataTokenType.SAMPLER3D;
            if (str == "samplerCube") return SAILDataTokenType.SAMPLERCUBE;
            //TODO: samplerArray
            Debug.Assert(false, "未知的数据类型");
            return SAILDataTokenType.error;
        }
        
        private static int MatchInputModifier(string str)
        {
            if (str == "uniform") return 0;
            if (str == "in") return 1;
            if (str == "out") return 2;
            if (str == "inout") return 3;
            Debug.Assert(false, "未知的输入修饰符");
            return 0;
        }
    }
}