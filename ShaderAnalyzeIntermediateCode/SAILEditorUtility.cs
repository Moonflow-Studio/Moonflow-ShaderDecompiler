using System;
using UnityEngine;

namespace moonflow_system.Tools.MFUtilityTools.GLSLCC
{
    public static class SAILEditorUtility
    {
        public static Color GetTokenTypeColor(SAILToken token)
        {
            if(token is SAILSymbolToken) return Color.white * token.GetUIIntensity();
            if(token is SAILMacroToken) return Color.magenta * token.GetUIIntensity();
            if(token is SAILNumberToken) return Color.yellow * token.GetUIIntensity();
            if(token is SAILVariableToken) return Color.cyan * token.GetUIIntensity();
            if (token is SAILPieceVariableToken)
            {
                var pToken = token as SAILPieceVariableToken;
                float intensity = pToken.link != null ? pToken.link.GetUIIntensity() : pToken.GetUIIntensity();
                return new Color(0.5f, 0.8f, 0.8f) * intensity;
            }
            if(token is SAILSpecialToken) return new Color(.8f, .5f, .5f) * token.GetUIIntensity();
            if(token is SAILLogicalToken) return new Color(0.2f, 0.5f, 0.58f) * token.GetUIIntensity();
            if(token is SAILFunctionTokenBase) return new Color(0.5f, 0.8f, 0.5f) * token.GetUIIntensity();
            if(token is SAILFunctionTokenExceed) return new Color(0.7f, 1f, 0.7f) * token.GetUIIntensity();
            return new Color(1f, 0.5f, 0.3f);
        }
        
    }
}