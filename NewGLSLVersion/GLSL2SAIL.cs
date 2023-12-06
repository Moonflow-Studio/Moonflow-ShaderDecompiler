using System.Collections.Generic;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public class GLSL2SAIL
    {
        private SAILData TransToSAIL(ref List<GLSLCCDecompileCore.GLSLSingleLine> lines)
        {
            var sailData = new SAILData();
            bool finishDeclaration = false;
            for (int i = 0; i < lines.Count; i++)
            {
                GLSLCCDecompileCore.GLSLSingleLine line = lines[i];
                if(line.tokens == null || line.tokens.Length == 0) continue;
                switch (line.glslLineType)
                {
                    case GLSLCCDecompileCore.GLSLLineType.inoutDeclaration:
                        if (finishDeclaration) Debug.Assert(false, "计算开始后不应该再有inout定义");
                        else
                        {
                            //TODO：识别分类转译
                        }
                        
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.uniformDeclaration:
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.tempDeclaration:
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.calculate:
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.logic:
                        break;
                    case GLSLCCDecompileCore.GLSLLineType.macro:
                        if(!finishDeclaration) continue;//TODO:计算开始之前的macro
                        sailData.calculationLines.Add(new SASingleLine()
                        {
                            hTokens = new []
                            {
                                new SAHToken()
                                {
                                    layer = 0,
                                    token = new SAILMacro()
                                    {
                                        macroType = MatchMacroType(line.tokens[0].tokenString),
                                        value = line.lineString.Replace(line.tokens[0].tokenString, "")
                                    }
                                }
                            }
                        });
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

        private SAILMacroType MatchMacroType(string str)
        {
            if(str == "#ifdef") return SAILMacroType.IFDEF;
            if(str == "#ifndef") return SAILMacroType.IFNDEF;
            if(str == "#else") return SAILMacroType.ELSE;
            if(str == "#endif") return SAILMacroType.ENDIF;
            if(str == "#define") return SAILMacroType.DEFINE;
            if(str == "#undef") return SAILMacroType.UNDEF;
            if(str == "#if") return SAILMacroType.IF;
            if(str == "#elif") return SAILMacroType.ELIF;
            return SAILMacroType.ERROR;
        }
    }
}