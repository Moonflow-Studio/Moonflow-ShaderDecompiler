using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
                        //TODO:有编译器会在函数开始执行后再生成临时变量，可能需要调整逻辑
                        // if (finishDeclaration) Debug.Assert(false, "计算开始后不应该再有临时变量定义");
                        // else
                        // {
                            CreateDeclaration(ref line, ref sailData);
                        // }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.calculate:
                        if (!finishDeclaration) Debug.Assert(false, "计算开始前不应有计算语句");
                        else
                        {
                            var singleline = new SAILSingleline();
                            singleline.lineString = "";
                            singleline.hTokens = new List<SAILHierToken>();
                            int layer = 0;
                            for (int j = 0; j < line.tokens.Length; j++)
                            {
                                var token = CreateCalculateToken(ref line.tokens[j], ref singleline, ref sailData, ref layer);
                                if(token.token == null) continue;
                                singleline.hTokens.Add(token);
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
                            singleline.hTokens = new List<SAILHierToken>();
                            int layer = 0;
                            for (int j = 0; j < line.tokens.Length; j++)
                            {
                                var token = CreateCalculateToken(ref line.tokens[j], ref singleline, ref sailData, ref layer);
                                singleline.hTokens.Add(token);
                                singleline.lineString += token.token?.tokenString;
                            }
                            sailData.calculationLines.Add(singleline);
                        }
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.macro:
                        if(!finishDeclaration) continue;//TODO:计算开始之前的macro
                        var macroLine = new SAILSingleline()
                        {
                            hTokens = new List<SAILHierToken>()
                        };
                        macroLine.hTokens.Add(new SAILHierToken()
                        {
                            layer = 0,
                            token = new SAILMacroToken()
                            {
                                macroTokenType = MatchMacroType(line.tokens[0].tokenString),
                                tokenString = line.tokens[0].ShowString(),
                                macroName = line.lineString.Replace(line.tokens[0].tokenString, "")
                                // value = line.tokens[0].tokenString
                            }
                        });
                        macroLine.hTokens.Add(new SAILHierToken()
                        {
                            layer = 0,
                            token = new SAILVariableToken()
                            {
                                tokenString = line.lineString.Replace(line.tokens[0].tokenString, ""),
                                tokenTypeName = "MacroDetail"
                            }
                        });
                        sailData.calculationLines.Add(macroLine);
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

        //TODO: 平台特殊函数可能返回多个token
        private static SAILHierToken CreateCalculateToken(ref GLSLToken lineToken, ref SAILSingleline line, ref SAILData data, ref int layer)
        {
            SAILHierToken hToken = new SAILHierToken();
            hToken.layer = layer;
            switch (lineToken.type)
            {
                case GLSLLexer.GLSLTokenType.instrFunc:
                {
                    SAILFunctionTokenBase functionToken = new SAILFunctionTokenBase();
                    bool matched = functionToken.MatchCommonFunctionIndex(lineToken.tokenString);
                    if (!matched)
                    {
                        matched = MatchDiffExpressFunction(ref lineToken.tokenString, ref functionToken);
                        if(!matched)
                        {
                            Debug.LogError( $"未知的函数 {lineToken.tokenString}");
                        }
                    }
                    hToken.token = functionToken;
                    hToken.token.tokenString = functionToken.GetName();
                    hToken.isNegative = lineToken.isNegative;
                    //TODO: 需要读取后面layer+1的内容
                    return hToken;
                }
                case GLSLLexer.GLSLTokenType.dataType:
                {
                    var type = MatchDataType(lineToken.tokenString);
                    hToken.token = SAILTokenFactory.CreateVariable(type.ToString().ToLower(), type);
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
                            tokenString = lineToken.tokenString,
                        };
                        hToken.isNegative = lineToken.isNegative;
                    }
                    return hToken;
                }
                case GLSLLexer.GLSLTokenType.name:
                    var variableToken = data.FindVariable(lineToken.tokenString);
                    if (variableToken != null)
                    {
                        hToken.token = variableToken;
                        return hToken;
                    }
                    else
                    {
                        if (lineToken.tokenString == "gl_Position")
                        {
                            hToken.token = data.outputPosition;
                            return hToken;
                        }
                        Debug.LogError($"name token '{lineToken.tokenString}' doesn't match any variable");
                        return hToken;
                    }
                case GLSLLexer.GLSLTokenType.partOfName:
                    //analyze lineToken by '.'
                    var nameTokens = lineToken.tokenString.Split('.');
                    //left of '.' is variable name, right of '.' is variable member
                    if (nameTokens.Length == 1)
                    {
                        //variable name
                        hToken.token = data.FindVariable(nameTokens[0]);
                    }
                    else
                    {
                        //variable member
                        var variable = data.FindVariable(nameTokens[0]);
                        hToken.token = new SAILPieceVariableToken()
                        {
                            channel = lineToken.tokenString.Replace(nameTokens[0]+'.',""),
                            link = variable,
                            tokenString = lineToken.tokenString
                        };
                    }
                    hToken.isNegative = lineToken.isNegative;
                    return hToken;
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
                            tokenString = lineToken.tokenString
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
                    var macroToken = new SAILMacroToken()
                    {
                        macroTokenType = MatchMacroType(lineToken.tokenString),
                        // value = lineToken.tokenString
                    };
                    macroToken.tokenString = macroToken.GetName();
                    hToken.token = macroToken;
                    return hToken;
                case GLSLLexer.GLSLTokenType.logicalOperator:
                    SAILLogicalToken logicalToken = new SAILLogicalToken();
                    logicalToken.Init(lineToken.tokenString);
                    logicalToken.tokenString = logicalToken.GetName();
                    hToken.token = logicalToken;
                    //TODO: 需要读取后面layer+1的内容
                    return hToken;
                case GLSLLexer.GLSLTokenType.unknown:
                    hToken.token = new SAILToken()
                    {
                        tokenString = lineToken.tokenString,
                    };
                    hToken.isNegative = lineToken.isNegative;
                    return hToken;
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
                //TODO: 矩阵定义需要通过变量名识别然后创建多个变量
                if (line.tokens[j].type == GLSLLexer.GLSLTokenType.name || line.tokens[j].type == GLSLLexer.GLSLTokenType.semanticRegex || line.tokens[j].type == GLSLLexer.GLSLTokenType.tempDeclarRegex)
                {
                    int multiDeclareCount = 1;
                    string realTokenName = line.tokens[j].tokenString;
                    if (line.tokens[j].type == GLSLLexer.GLSLTokenType.name ||
                        line.tokens[j].type == GLSLLexer.GLSLTokenType.tempDeclarRegex)
                    {
                        multiDeclareCount = GetMultiDelcareCount(line.tokens[j].tokenString, out realTokenName);
                    }
                    if (dataTokenType != SAILDataTokenType.error)
                    {
                        switch (modifierType)
                        {
                            case 0:
                                if (dataTokenType == SAILDataTokenType.SAMPLER2D ||
                                    dataTokenType == SAILDataTokenType.SAMPLER3D ||
                                    dataTokenType == SAILDataTokenType.SAMPLERCUBE)
                                {
                                    if (multiDeclareCount > 1)
                                    {
                                        for (int i = 0; i < multiDeclareCount; i++)
                                        {
                                            sailData.glbVar.Add(SAILTokenFactory.CreateTexture($"{realTokenName}[{i}]", dataTokenType));
                                        }
                                    }
                                    else
                                    {
                                        sailData.glbVar.Add(SAILTokenFactory.CreateTexture(realTokenName, dataTokenType));
                                    }
                                }
                                else
                                {
                                    if (multiDeclareCount > 1)
                                    {
                                        for (int i = 0; i < multiDeclareCount; i++)
                                        {
                                            sailData.glbVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                        }
                                    }
                                    else
                                    {
                                        sailData.glbVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                    }
                                }
                                return;
                            case 1:
                                if (multiDeclareCount > 1)
                                {
                                    for (int i = 0; i < multiDeclareCount; i++)
                                    {
                                        sailData.inVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                    }
                                }
                                else
                                {
                                    sailData.inVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                }
                                return;
                            case 2:
                                if (multiDeclareCount > 1)
                                {
                                    for (int i = 0; i < multiDeclareCount; i++)
                                    {
                                        sailData.outVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                    }
                                }
                                else
                                {
                                    sailData.outVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                }
                                return;
                            case 3:
                                if (multiDeclareCount > 1)
                                {
                                    for (int i = 0; i < multiDeclareCount; i++)
                                    {
                                        sailData.inVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                        sailData.outVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                    }
                                }
                                else
                                {
                                    sailData.inVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                    sailData.outVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                }
                                return;
                            case -1:
                                if (multiDeclareCount > 1)
                                {
                                    for (int i = 0; i < multiDeclareCount; i++)
                                    {
                                        sailData.tempVar.Add(SAILTokenFactory.CreateVariable($"{realTokenName}[{i}]", dataTokenType));
                                    }
                                }
                                else
                                {
                                    sailData.tempVar.Add(SAILTokenFactory.CreateVariable(realTokenName, dataTokenType));
                                }
                                return;
                        }
                    }
                }
            }
            Debug.LogError($"未知的声明: {line.lineString} - modifierType:{modifierType} dataType:{dataTokenType}");
        }

        private static int GetMultiDelcareCount(string tokenString, out string newTokenString)
        {
            newTokenString = tokenString;
            if (!tokenString.Contains('['))
            {
                return 1;
            }
            else
            {
                Regex regex = new Regex(".[/[0-9/]]");
                Match matches = regex.Match(tokenString);
                string channelString = matches.Groups[0].Value;
                channelString = channelString.Replace("[","").Replace("]","");
                int channel = Convert.ToInt32(channelString);
                newTokenString = newTokenString.Replace(matches.Groups[0].Value, "");
                return channel;
            }
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
            if (str == "bvec2") return SAILDataTokenType.BOOL2;
            if (str == "bvec3") return SAILDataTokenType.BOOL3;
            if (str == "bvec4") return SAILDataTokenType.BOOL4;
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
            return -1;
        }

        private static bool MatchDiffExpressFunction(ref string str, ref SAILFunctionTokenBase sail)
        {
            switch (str)
            {
                case "texture" : return sail.MatchCommonFunctionIndex("sampleTexture");
                case "fract" : return sail.MatchCommonFunctionIndex("frac");
                case "inversesqrt" : return sail.MatchCommonFunctionIndex("rsqrt");
                //TODO：应当返回成一个texture和一个lod sampler
                case "textureLod" : return sail.MatchCommonFunctionIndex("sampleTexture");
            }
            return false;
        }
    }
}