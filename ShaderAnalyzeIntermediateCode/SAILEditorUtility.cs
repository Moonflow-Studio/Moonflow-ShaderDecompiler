using System;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILEditorUtility
    {
        public static Color GetTokenTypeColor(SAILToken token)
        {
            if(token is SAILSymbolToken) return Color.white;
            if(token is SAILMacroToken) return Color.magenta;
            if(token is SAILNumberToken) return Color.yellow;
            if(token is SAILVariableToken) return Color.cyan;
            if(token is SAILPieceVariableToken) return new Color(0.5f, 0.8f, 0.8f);
            if(token is SAILSpecialToken) return Color.red;
            if(token is SAILLogicalToken) return new Color(0.2f, 0.5f, 0.58f);
            if(token is SAILFunctionTokenBase) return new Color(0.5f, 0.8f, 0.5f);
            if(token is SAILFunctionTokenExceed) return new Color(0.7f, 1f, 0.7f);
            return Color.black;
        }
        
        public static float GetTokenTypeWidth(SAILToken token)
        {
            if(token is SAILSymbolToken) return 20;
            if(token is SAILMacroToken) return 75;
            if(token is SAILNumberToken) return 100;
            if(token is SAILVariableToken) return 100;
            if(token is SAILLogicalToken) return 50;
            if(token is SAILPieceVariableToken) return 120;
            if(token is SAILFunctionTokenBase) return 100;
            if(token is SAILFunctionTokenExceed) return 100;
            if(token is SAILSpecialToken) return 150;
            return 100;
        }
    }
}